using TicketsApp.Domain.Entities;

namespace TicketsApp.Application.Common.Interfaces;

public interface ICatalogoService
{
    Task<List<Categoria>> ObtenerCategoriasAsync(bool soloActivas = true);
    Task<Categoria?> ObtenerCategoriaPorIdAsync(int id);
    Task<Categoria> CrearCategoriaAsync(Categoria categoria);
    Task<bool> ActualizarCategoriaAsync(Categoria categoria);
    Task<bool> AlternarEstadoCategoriaAsync(int id);

    Task<List<Ubicacion>> ObtenerUbicacionesAsync(bool soloActivas = true);
    Task<Ubicacion?> ObtenerUbicacionPorIdAsync(int id);
    Task<Ubicacion> CrearUbicacionAsync(Ubicacion ubicacion);
    Task<bool> ActualizarUbicacionAsync(Ubicacion ubicacion);
    Task<bool> AlternarEstadoUbicacionAsync(int id);
}
