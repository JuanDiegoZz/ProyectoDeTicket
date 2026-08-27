using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketsApp.Application.ViewModels;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool EsAdministrador()
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        return rol == RolUsuario.Administrador.ToString();
    }

    [HttpGet("tecnicos")]
    public async Task<IActionResult> ObtenerTecnicos()
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        var tecnicos = await _context.Usuarios
            .Where(u => u.Rol == RolUsuario.Tecnico)
            .OrderByDescending(u => u.Activo)
            .ThenBy(u => u.NombreCompleto)
            .ToListAsync();

        return Ok(tecnicos);
    }

    [HttpPost("crear-tecnico")]
    public async Task<IActionResult> CrearTecnico([FromBody] RegistroViewModel model)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existe = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
        if (existe)
        {
            return BadRequest(new { mensaje = "El correo del técnico ya se encuentra registrado." });
        }

        var nuevoTecnico = new Usuario
        {
            NombreCompleto = model.NombreCompleto.Trim(),
            Email = model.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Rol = RolUsuario.Tecnico,
            TipoSolicitante = TipoSolicitante.Administrativo,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoTecnico);
        await _context.SaveChangesAsync();

        return Ok(nuevoTecnico);
    }

    [HttpPost("tecnicos/{id}/alternar-estado")]
    public async Task<IActionResult> AlternarEstadoTecnico(int id)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        var tecnico = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Rol == RolUsuario.Tecnico);
        if (tecnico == null) return NotFound(new { mensaje = "Técnico no encontrado." });

        tecnico.Activo = !tecnico.Activo;

        if (!tecnico.Activo)
        {
            var ticketsAsignados = await _context.Tickets
                .Where(t => t.TecnicoAsignadoId == tecnico.Id && t.Estado != EstadoTicket.Resuelto && t.Estado != EstadoTicket.Cancelado)
                .ToListAsync();

            foreach (var t in ticketsAsignados)
            {
                t.TecnicoAsignadoId = null;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Estado del técnico actualizado correctamente.", activo = tecnico.Activo });
    }
}
