using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Application.ViewModels;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailClassifierService _emailClassifier;

    public AuthController(ApplicationDbContext context, IEmailClassifierService emailClassifier)
    {
        _context = context;
        _emailClassifier = emailClassifier;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas. Verifica tu correo y contraseña." });
        }

        if (!usuario.Activo)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Tu cuenta se encuentra inactiva. Contacta al administrador." });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("TipoSolicitante", usuario.TipoSolicitante.ToString())
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
            authProperties);

        return Ok(new
        {
            id = usuario.Id,
            nombreCompleto = usuario.NombreCompleto,
            email = usuario.Email,
            rol = usuario.Rol.ToString(),
            tipoSolicitante = usuario.TipoSolicitante.ToString()
        });
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errores = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { mensaje = errores });
        }

        var existe = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
        if (existe)
        {
            return BadRequest(new { mensaje = "El correo electrónico ya se encuentra registrado. Intenta con otro correo o inicia sesión." });
        }

        var clasificacion = _emailClassifier.ClasificarEmail(model.Email);

        var nuevoUsuario = new Usuario
        {
            NombreCompleto = model.NombreCompleto.Trim(),
            Email = model.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Rol = RolUsuario.Solicitante,
            TipoSolicitante = clasificacion.Tipo,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario registrado correctamente", id = nuevoUsuario.Id });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { mensaje = "Sesión cerrada exitosamente." });
    }

    [HttpGet("me")]
    public IActionResult ObtenerUsuarioActual()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { mensaje = "No hay sesión activa." });
        }

        return Ok(new
        {
            id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            nombreCompleto = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            rol = User.FindFirstValue(ClaimTypes.Role),
            tipoSolicitante = User.FindFirstValue("TipoSolicitante")
        });
    }
}
