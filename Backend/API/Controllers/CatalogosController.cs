using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    private bool EsAdministrador()
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        return rol == RolUsuario.Administrador.ToString();
    }

    // ==========================================
    // CATEGORÍAS
    // ==========================================

    [HttpGet("categorias")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerCategorias([FromQuery] bool soloActivas = true)
    {
        var categorias = await _catalogoService.ObtenerCategoriasAsync(soloActivas);
        return Ok(categorias);
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria([FromBody] Categoria categoria)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        var nueva = await _catalogoService.CrearCategoriaAsync(categoria);
        return Ok(nueva);
    }

    [HttpPut("categorias/{id}")]
    public async Task<IActionResult> EditarCategoria(int id, [FromBody] Categoria categoria)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        categoria.Id = id;
        var resultado = await _catalogoService.ActualizarCategoriaAsync(categoria);
        if (!resultado) return NotFound(new { mensaje = "Categoría no encontrada." });

        return Ok(new { mensaje = "Categoría actualizada correctamente." });
    }

    [HttpPost("categorias/{id}/alternar-estado")]
    public async Task<IActionResult> AlternarEstadoCategoria(int id)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        var resultado = await _catalogoService.AlternarEstadoCategoriaAsync(id);
        if (!resultado) return NotFound(new { mensaje = "Categoría no encontrada." });

        return Ok(new { mensaje = "Estado de la categoría alternado exitosamente." });
    }

    // ==========================================
    // UBICACIONES
    // ==========================================

    [HttpGet("ubicaciones")]
    [AllowAnonymous]
    public async Task<IActionResult> ObtenerUbicaciones([FromQuery] bool soloActivas = true)
    {
        var ubicaciones = await _catalogoService.ObtenerUbicacionesAsync(soloActivas);
        return Ok(ubicaciones);
    }

    [HttpPost("ubicaciones")]
    public async Task<IActionResult> CrearUbicacion([FromBody] Ubicacion ubicacion)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        if (string.IsNullOrWhiteSpace(ubicacion.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la ubicación es obligatorio." });
        }

        var nueva = await _catalogoService.CrearUbicacionAsync(ubicacion);
        return Ok(nueva);
    }

    [HttpPut("ubicaciones/{id}")]
    public async Task<IActionResult> EditarUbicacion(int id, [FromBody] Ubicacion ubicacion)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        ubicacion.Id = id;
        var resultado = await _catalogoService.ActualizarUbicacionAsync(ubicacion);
        if (!resultado) return NotFound(new { mensaje = "Ubicación no encontrada." });

        return Ok(new { mensaje = "Ubicación actualizada correctamente." });
    }

    [HttpPost("ubicaciones/{id}/alternar-estado")]
    public async Task<IActionResult> AlternarEstadoUbicacion(int id)
    {
        if (!EsAdministrador()) return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = "Acceso denegado." });

        var resultado = await _catalogoService.AlternarEstadoUbicacionAsync(id);
        if (!resultado) return NotFound(new { mensaje = "Ubicación no encontrada." });

        return Ok(new { mensaje = "Estado de la ubicación alternado exitosamente." });
    }
}
