using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Application.ViewModels;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailClassifierService _emailClassifier;

    public AccountController(ApplicationDbContext context, IEmailClassifierService emailClassifier)
    {
        _context = context;
        _emailClassifier = emailClassifier;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (usuario == null || !usuario.Activo || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas o la cuenta se encuentra desactivada.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("TipoSolicitante", usuario.TipoSolicitante.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Tickets");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var clasificacion = _emailClassifier.ClasificarEmail(model.Email);
        if (!clasificacion.EsValido)
        {
            ModelState.AddModelError("Email", clasificacion.MensajeError);
            return View(model);
        }

        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (emailExiste)
        {
            ModelState.AddModelError("Email", "El correo ya se encuentra registrado en el sistema.");
            return View(model);
        }

        var nuevoUsuario = new Usuario
        {
            NombreCompleto = model.NombreCompleto.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Rol = RolUsuario.Solicitante,
            TipoSolicitante = clasificacion.Tipo,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Registro exitoso. Inicia sesión con tus credenciales.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // Panel para Gestión y Baja de Técnicos
    [HttpGet]
    public async Task<IActionResult> GestionTecnicos()
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        if (rol != RolUsuario.Administrador.ToString())
        {
            return Forbid();
        }

        var tecnicos = await _context.Usuarios
            .Where(u => u.Rol == RolUsuario.Tecnico)
            .OrderByDescending(u => u.Activo)
            .ThenBy(u => u.NombreCompleto)
            .ToListAsync();

        return View(tecnicos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltaTecnico(RegistroViewModel model)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        if (rol != RolUsuario.Administrador.ToString())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Datos incompletos para registrar al técnico.";
            return RedirectToAction(nameof(GestionTecnicos));
        }

        var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
        if (emailExiste)
        {
            TempData["ErrorMessage"] = "El correo ya está registrado en el sistema.";
            return RedirectToAction(nameof(GestionTecnicos));
        }

        var nuevoTecnico = new Usuario
        {
            NombreCompleto = model.NombreCompleto.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Rol = RolUsuario.Tecnico,
            TipoSolicitante = TipoSolicitante.Administrativo,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoTecnico);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Técnico '{nuevoTecnico.NombreCompleto}' dado de alta exitosamente.";
        return RedirectToAction(nameof(GestionTecnicos));
    }

    // POST: /Account/AlternarEstadoTecnico (Dar de baja o reactivar técnico)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarEstadoTecnico(int tecnicoId)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        if (rol != RolUsuario.Administrador.ToString())
        {
            return Forbid();
        }

        var tecnico = await _context.Usuarios.FindAsync(tecnicoId);
        if (tecnico == null || tecnico.Rol != RolUsuario.Tecnico)
        {
            return NotFound();
        }

        tecnico.Activo = !tecnico.Activo;
        await _context.SaveChangesAsync();

        var estadoTexto = tecnico.Activo ? "Reactivado y Habilitado" : "Dado de Baja (Desactivado)";
        TempData["SuccessMessage"] = $"El técnico '{tecnico.NombreCompleto}' ha sido {estadoTexto}.";

        return RedirectToAction(nameof(GestionTecnicos));
    }
}
