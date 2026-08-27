using Microsoft.EntityFrameworkCore;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Domain.Entities;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Infrastructure.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ApplicationDbContext _context;

    public CatalogoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> ObtenerCategoriasAsync(bool soloActivas = true)
    {
        var query = _context.Categorias.AsQueryable();
        if (soloActivas) query = query.Where(c => c.Activo);
        return await query.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task<Categoria> CrearCategoriaAsync(Categoria categoria)
    {
        categoria.Nombre = categoria.Nombre.Trim();
        categoria.Descripcion = categoria.Descripcion?.Trim();
        categoria.Activo = true;
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
    {
        var existente = await _context.Categorias.FindAsync(categoria.Id);
        if (existente == null) return false;

        existente.Nombre = categoria.Nombre.Trim();
        existente.Descripcion = categoria.Descripcion?.Trim();
        existente.Activo = categoria.Activo;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AlternarEstadoCategoriaAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return false;

        categoria.Activo = !categoria.Activo;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Ubicacion>> ObtenerUbicacionesAsync(bool soloActivas = true)
    {
        var query = _context.Ubicaciones.AsQueryable();
        if (soloActivas) query = query.Where(u => u.Activo);
        return await query.OrderBy(u => u.Nombre).ToListAsync();
    }

    public async Task<Ubicacion?> ObtenerUbicacionPorIdAsync(int id)
    {
        return await _context.Ubicaciones.FindAsync(id);
    }

    public async Task<Ubicacion> CrearUbicacionAsync(Ubicacion ubicacion)
    {
        ubicacion.Nombre = ubicacion.Nombre.Trim();
        ubicacion.Activo = true;
        _context.Ubicaciones.Add(ubicacion);
        await _context.SaveChangesAsync();
        return ubicacion;
    }

    public async Task<bool> ActualizarUbicacionAsync(Ubicacion ubicacion)
    {
        var existente = await _context.Ubicaciones.FindAsync(ubicacion.Id);
        if (existente == null) return false;

        existente.Nombre = ubicacion.Nombre.Trim();
        existente.Activo = ubicacion.Activo;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AlternarEstadoUbicacionAsync(int id)
    {
        var ubicacion = await _context.Ubicaciones.FindAsync(id);
        if (ubicacion == null) return false;

        ubicacion.Activo = !ubicacion.Activo;
        await _context.SaveChangesAsync();
        return true;
    }
}
