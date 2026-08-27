using Microsoft.EntityFrameworkCore;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Application.Common.Models;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;

    public TicketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> ObtenerPorIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.TecnicoAsignado)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.Notas)
                .ThenInclude(n => n.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Ticket>> ObtenerTicketsSolicitanteAsync(int solicitanteId)
    {
        return await _context.Tickets
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.TecnicoAsignado)
            .Where(t => t.SolicitanteId == solicitanteId)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Ticket>> ObtenerTicketsTecnicoAsync(int tecnicoId)
    {
        return await _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.TecnicoAsignado)
            .Where(t => t.TecnicoAsignadoId == tecnicoId || t.TecnicoAsignadoId == null)
            .OrderByDescending(t => t.Prioridad == PrioridadTicket.Alta)
            .ThenByDescending(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Ticket>> ObtenerTodosTicketsAsync()
    {
        return await _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.TecnicoAsignado)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<PagedResult<Ticket>> ObtenerTicketsPaginadosAsync(
        string? busqueda = null,
        EstadoTicket? estado = null,
        PrioridadTicket? prioridad = null,
        int? categoriaId = null,
        int? ubicacionId = null,
        int pagina = 1,
        int tamanoPagina = 10,
        string? orden = null)
    {
        var query = _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.TecnicoAsignado)
            .AsQueryable();

        // 1. Filtrado por texto libre
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(t => 
                t.Titulo.ToLower().Contains(texto) ||
                t.Descripcion.ToLower().Contains(texto) ||
                t.Solicitante.NombreCompleto.ToLower().Contains(texto) ||
                t.Solicitante.Email.ToLower().Contains(texto) ||
                (t.DetalleAula != null && t.DetalleAula.ToLower().Contains(texto)));
        }

        // 2. Filtros estructurados
        if (estado.HasValue)
        {
            query = query.Where(t => t.Estado == estado.Value);
        }

        if (prioridad.HasValue)
        {
            query = query.Where(t => t.Prioridad == prioridad.Value);
        }

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(t => t.CategoriaId == categoriaId.Value);
        }

        if (ubicacionId.HasValue && ubicacionId.Value > 0)
        {
            query = query.Where(t => t.UbicacionId == ubicacionId.Value);
        }

        // 3. Ordenamiento
        query = orden switch
        {
            "fecha_asc" => query.OrderBy(t => t.FechaCreacion),
            "prioridad_desc" => query.OrderByDescending(t => t.Prioridad).ThenByDescending(t => t.FechaCreacion),
            "prioridad_asc" => query.OrderBy(t => t.Prioridad).ThenByDescending(t => t.FechaCreacion),
            "folio_asc" => query.OrderBy(t => t.Id),
            "folio_desc" => query.OrderByDescending(t => t.Id),
            _ => query.OrderByDescending(t => t.FechaCreacion)
        };

        // 4. Paginación en servidor
        var totalItems = await query.CountAsync();
        var paginaAjustada = Math.Max(1, pagina);
        var items = await query
            .Skip((paginaAjustada - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return new PagedResult<Ticket>(items, totalItems, paginaAjustada, tamanoPagina);
    }

    public async Task<Ticket> CrearTicketAsync(Ticket ticket)
    {
        ticket.FechaCreacion = DateTime.UtcNow;
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<bool> CambiarEstadoAsync(int ticketId, EstadoTicket nuevoEstado, int usuarioId, string? nota)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        if (ticket.TecnicoAsignadoId == null)
        {
            ticket.TecnicoAsignadoId = usuarioId;
        }

        ticket.Estado = nuevoEstado;
        ticket.FechaActualizacion = DateTime.UtcNow;

        if (nuevoEstado == EstadoTicket.Resuelto)
        {
            ticket.FechaResolucion = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(nota))
        {
            var nuevaNota = new NotaTicket
            {
                TicketId = ticket.Id,
                UsuarioId = usuarioId,
                Mensaje = $"[Cambio a {nuevoEstado}]: {nota.Trim()}",
                FechaCreacion = DateTime.UtcNow
            };
            _context.NotasTicket.Add(nuevaNota);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReasignarTecnicoAsync(int ticketId, int? nuevoTecnicoId, int usuarioAdminId, string? motivo)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        Usuario? nuevoTecnico = null;
        if (nuevoTecnicoId.HasValue)
        {
            nuevoTecnico = await _context.Usuarios.FindAsync(nuevoTecnicoId.Value);
        }

        ticket.TecnicoAsignadoId = nuevoTecnicoId;
        ticket.FechaActualizacion = DateTime.UtcNow;

        var mensajeNota = nuevoTecnico != null
            ? $"[Reasignación Administrativa]: Asignado a '{nuevoTecnico.NombreCompleto}'. Motivo: {motivo ?? "Sin motivo especificado"}."
            : $"[Reasignación Administrativa]: Ticket liberado a la cola general. Motivo: {motivo ?? "Sin motivo especificado"}.";

        var nuevaNota = new NotaTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioAdminId,
            Mensaje = mensajeNota,
            FechaCreacion = DateTime.UtcNow
        };
        _context.NotasTicket.Add(nuevaNota);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CalificarTicketAsync(int ticketId, int estrellas, string? comentario, int solicitanteId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null || ticket.SolicitanteId != solicitanteId) return false;

        ticket.CalificacionSatisfaccion = Math.Clamp(estrellas, 1, 5);
        ticket.ComentarioSatisfaccion = comentario?.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AgregarNotaAsync(int ticketId, int usuarioId, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje)) return false;

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        var nuevaNota = new NotaTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioId,
            Mensaje = mensaje.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _context.NotasTicket.Add(nuevaNota);
        ticket.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarPrioridadAsync(int ticketId, PrioridadTicket nuevaPrioridad, int usuarioAdminId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        var prioridadAnterior = ticket.Prioridad;
        ticket.Prioridad = nuevaPrioridad;
        ticket.FechaActualizacion = DateTime.UtcNow;

        var nuevaNota = new NotaTicket
        {
            TicketId = ticket.Id,
            UsuarioId = usuarioAdminId,
            Mensaje = $"[Cambio de Prioridad]: Modificada de '{prioridadAnterior}' a '{nuevaPrioridad}' por administración.",
            FechaCreacion = DateTime.UtcNow
        };
        _context.NotasTicket.Add(nuevaNota);

        await _context.SaveChangesAsync();
        return true;
    }
}
