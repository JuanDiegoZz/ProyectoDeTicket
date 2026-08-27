using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Controllers;

public class CatalogosController : Controller
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
    // GESTIÓN DE CATEGORÍAS
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Categorias()
    {
        if (!EsAdministrador()) return Forbid();
        var categorias = await _catalogoService.ObtenerCategoriasAsync(soloActivas: false);
        return View(categorias);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(Categoria categoria)
    {
        if (!EsAdministrador()) return Forbid();

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            TempData["ErrorMessage"] = "El nombre de la categoría es obligatorio.";
            return RedirectToAction(nameof(Categorias));
        }

        await _catalogoService.CrearCategoriaAsync(categoria);
        TempData["SuccessMessage"] = $"Categoría '{categoria.Nombre}' creada exitosamente.";
        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(Categoria categoria)
    {
        if (!EsAdministrador()) return Forbid();

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            TempData["ErrorMessage"] = "El nombre de la categoría es obligatorio.";
            return RedirectToAction(nameof(Categorias));
        }

        var resultado = await _catalogoService.ActualizarCategoriaAsync(categoria);
        if (!resultado)
        {
            TempData["ErrorMessage"] = "No se encontró la categoría a editar.";
        }
        else
        {
            TempData["SuccessMessage"] = "Categoría actualizada correctamente.";
        }

        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarEstadoCategoria(int id)
    {
        if (!EsAdministrador()) return Forbid();

        var resultado = await _catalogoService.AlternarEstadoCategoriaAsync(id);
        if (!resultado)
        {
            TempData["ErrorMessage"] = "No se encontró la categoría.";
        }
        else
        {
            TempData["SuccessMessage"] = "Estado de la categoría modificado exitosamente.";
        }

        return RedirectToAction(nameof(Categorias));
    }

    // ==========================================
    // GESTIÓN DE UBICACIONES
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Ubicaciones()
    {
        if (!EsAdministrador()) return Forbid();
        var ubicaciones = await _catalogoService.ObtenerUbicacionesAsync(soloActivas: false);
        return View(ubicaciones);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUbicacion(Ubicacion ubicacion)
    {
        if (!EsAdministrador()) return Forbid();

        if (string.IsNullOrWhiteSpace(ubicacion.Nombre))
        {
            TempData["ErrorMessage"] = "El nombre de la ubicación es obligatorio.";
            return RedirectToAction(nameof(Ubicaciones));
        }

        await _catalogoService.CrearUbicacionAsync(ubicacion);
        TempData["SuccessMessage"] = $"Ubicación '{ubicacion.Nombre}' creada exitosamente.";
        return RedirectToAction(nameof(Ubicaciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUbicacion(Ubicacion ubicacion)
    {
        if (!EsAdministrador()) return Forbid();

        if (string.IsNullOrWhiteSpace(ubicacion.Nombre))
        {
            TempData["ErrorMessage"] = "El nombre de la ubicación es obligatorio.";
            return RedirectToAction(nameof(Ubicaciones));
        }

        var resultado = await _catalogoService.ActualizarUbicacionAsync(ubicacion);
        if (!resultado)
        {
            TempData["ErrorMessage"] = "No se encontró la ubicación a editar.";
        }
        else
        {
            TempData["SuccessMessage"] = "Ubicación actualizada correctamente.";
        }

        return RedirectToAction(nameof(Ubicaciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarEstadoUbicacion(int id)
    {
        if (!EsAdministrador()) return Forbid();

        var resultado = await _catalogoService.AlternarEstadoUbicacionAsync(id);
        if (!resultado)
        {
            TempData["ErrorMessage"] = "No se encontró la ubicación.";
        }
        else
        {
            TempData["SuccessMessage"] = "Estado de la ubicación modificado exitosamente.";
        }

        return RedirectToAction(nameof(Ubicaciones));
    }
}
