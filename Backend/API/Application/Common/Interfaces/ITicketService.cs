using TicketsApp.Application.Common.Models;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Application.Common.Interfaces;

public interface ITicketService
{
    Task<Ticket?> ObtenerPorIdAsync(int id);
    Task<List<Ticket>> ObtenerTicketsSolicitanteAsync(int solicitanteId);
    Task<List<Ticket>> ObtenerTicketsTecnicoAsync(int tecnicoId);
    Task<List<Ticket>> ObtenerTodosTicketsAsync();
    
    // Consulta Paginada y Filtrada desde Servidor / PostgreSQL
    Task<PagedResult<Ticket>> ObtenerTicketsPaginadosAsync(
        string? busqueda = null,
        EstadoTicket? estado = null,
        PrioridadTicket? prioridad = null,
        int? categoriaId = null,
        int? ubicacionId = null,
        int pagina = 1,
        int tamanoPagina = 10,
        string? orden = null);

    Task<Ticket> CrearTicketAsync(Ticket ticket);
    Task<bool> CambiarEstadoAsync(int ticketId, EstadoTicket nuevoEstado, int usuarioId, string? nota);
    Task<bool> ReasignarTecnicoAsync(int ticketId, int? nuevoTecnicoId, int usuarioAdminId, string? motivo);
    Task<bool> CalificarTicketAsync(int ticketId, int estrellas, string? comentario, int solicitanteId);
    Task<bool> AgregarNotaAsync(int ticketId, int usuarioId, string mensaje);
    Task<bool> ActualizarPrioridadAsync(int ticketId, PrioridadTicket nuevaPrioridad, int usuarioAdminId);
}
