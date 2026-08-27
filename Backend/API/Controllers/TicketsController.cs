using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Application.ViewModels;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITicketService _ticketService;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailClassifierService _emailClassifier;

    public TicketsController(
        ApplicationDbContext context,
        ITicketService ticketService,
        IWebHostEnvironment environment,
        IEmailClassifierService emailClassifier)
    {
        _context = context;
        _ticketService = ticketService;
        _environment = environment;
        _emailClassifier = emailClassifier;
    }

    private (int? Id, RolUsuario? Rol, string? Email) ObtenerSesionUsuario()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rolClaim = User.FindFirstValue(ClaimTypes.Role);
        var emailClaim = User.FindFirstValue(ClaimTypes.Email);

        if (int.TryParse(idClaim, out int id))
        {
            if (Enum.TryParse<RolUsuario>(rolClaim, true, out var rol))
            {
                return (id, rol, emailClaim);
            }
        }
        return (null, null, null);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTickets(
        [FromQuery] string? busqueda = null,
        [FromQuery] EstadoTicket? estado = null,
        [FromQuery] PrioridadTicket? prioridad = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] int? ubicacionId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? orden = null)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null) return Unauthorized();

        if (userRol == RolUsuario.Administrador)
        {
            var pagedTickets = await _ticketService.ObtenerTicketsPaginadosAsync(
                busqueda, estado, prioridad, categoriaId, ubicacionId, fechaInicio, fechaFin, pagina, tamanoPagina, orden);
            return Ok(pagedTickets);
        }

        if (userRol == RolUsuario.Tecnico)
        {
            var ticketsTecnico = await _ticketService.ObtenerTicketsTecnicoAsync(userId.Value);
            return Ok(ticketsTecnico);
        }

        var ticketsSolicitante = await _ticketService.ObtenerTicketsSolicitanteAsync(userId.Value);
        return Ok(ticketsSolicitante);
    }

    [HttpGet("dashboard-admin")]
    public async Task<IActionResult> ObtenerDashboardAdmin(
        [FromQuery] string? busqueda = null,
        [FromQuery] EstadoTicket? estado = null,
        [FromQuery] PrioridadTicket? prioridad = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] int? ubicacionId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador) return StatusCode(StatusCodes.Status403Forbidden);

        var query = _context.Tickets.AsQueryable();

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

        if (estado.HasValue) query = query.Where(t => t.Estado == estado.Value);
        if (prioridad.HasValue) query = query.Where(t => t.Prioridad == prioridad.Value);
        if (categoriaId.HasValue && categoriaId.Value > 0) query = query.Where(t => t.CategoriaId == categoriaId.Value);
        if (ubicacionId.HasValue && ubicacionId.Value > 0) query = query.Where(t => t.UbicacionId == ubicacionId.Value);

        if (fechaInicio.HasValue)
        {
            var inicioUtc = DateTime.SpecifyKind(fechaInicio.Value.Date, DateTimeKind.Utc);
            query = query.Where(t => t.FechaCreacion >= inicioUtc);
        }

        if (fechaFin.HasValue)
        {
            var finUtc = DateTime.SpecifyKind(fechaFin.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(t => t.FechaCreacion <= finUtc);
        }

        var total = await query.CountAsync();
        var abiertos = await query.CountAsync(t => t.Estado == EstadoTicket.Abierto);
        var enProgreso = await query.CountAsync(t => t.Estado == EstadoTicket.EnProgreso);
        var resueltos = await query.CountAsync(t => t.Estado == EstadoTicket.Resuelto);

        var fallasUbicacion = await query
            .GroupBy(t => t.Ubicacion!.Nombre)
            .Select(g => new MetricaAreaViewModel { Ubicacion = g.Key, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        var fallasCategoria = await query
            .GroupBy(t => t.Categoria!.Nombre)
            .Select(g => new MetricaCategoriaViewModel { Categoria = g.Key, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();

        var topSolicitantes = await query
            .GroupBy(t => new { t.Solicitante!.NombreCompleto, t.Solicitante.Email })
            .Select(g => new MetricaUsuarioViewModel
            {
                Nombre = g.Key.NombreCompleto,
                Email = g.Key.Email,
                TotalReportados = g.Count()
            })
            .OrderByDescending(x => x.TotalReportados)
            .Take(5)
            .ToListAsync();

        var eficienciaTecnicos = await _context.Usuarios
            .Where(u => u.Rol == RolUsuario.Tecnico && u.Activo)
            .Select(u => new MetricaTecnicoViewModel
            {
                Nombre = u.NombreCompleto,
                Resueltos = u.TicketsAsignados.Count(t => t.Estado == EstadoTicket.Resuelto),
                EnProgreso = u.TicketsAsignados.Count(t => t.Estado == EstadoTicket.EnProgreso)
            })
            .ToListAsync();

        return Ok(new
        {
            totalTickets = total,
            ticketsAbiertos = abiertos,
            ticketsEnProgreso = enProgreso,
            ticketsResueltos = resueltos,
            fallasPorUbicacion = fallasUbicacion,
            fallasPorCategoria = fallasCategoria,
            topSolicitantes = topSolicitantes,
            eficienciaTecnicos = eficienciaTecnicos
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null) return Unauthorized();

        var ticket = await _ticketService.ObtenerPorIdAsync(id);
        if (ticket == null) return NotFound(new { mensaje = "Ticket no encontrado." });

        if (userRol == RolUsuario.Solicitante && ticket.SolicitanteId != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] CrearTicketViewModel model)
    {
        var (userId, _, userEmail) = ObtenerSesionUsuario();
        if (userId == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            var errores = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { mensaje = errores });
        }

        string? rutaGuardada = null;

        if (model.ArchivoEvidencia != null && model.ArchivoEvidencia.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var extension = Path.GetExtension(model.ArchivoEvidencia.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new { mensaje = "Extensión de archivo no permitida." });
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ArchivoEvidencia.CopyToAsync(fileStream);
            }

            rutaGuardada = $"/uploads/{uniqueFileName}";
        }

        var clasificacion = _emailClassifier.ClasificarEmail(userEmail ?? string.Empty);
        var prioridadAsignada = clasificacion.PrioridadSugerida;

        var ticket = new Ticket
        {
            Titulo = model.Titulo.Trim(),
            Descripcion = model.Descripcion.Trim(),
            CategoriaId = model.CategoriaId,
            UbicacionId = model.UbicacionId,
            DetalleAula = model.DetalleAula?.Trim(),
            RutaEvidencia = rutaGuardada,
            SolicitanteId = userId.Value,
            Prioridad = prioridadAsignada,
            Estado = EstadoTicket.Abierto,
            FechaCreacion = DateTime.UtcNow
        };

        await _ticketService.CrearTicketAsync(ticket);
        return Ok(ticket);
    }

    [HttpPost("{id}/cambiar-estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null || (userRol != RolUsuario.Tecnico && userRol != RolUsuario.Administrador))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var resultado = await _ticketService.CambiarEstadoAsync(id, dto.NuevoEstado, userId.Value, dto.Nota);
        if (!resultado) return NotFound();

        return Ok(new { mensaje = "Estado de ticket actualizado correctamente." });
    }

    [HttpPost("{id}/reasignar")]
    public async Task<IActionResult> ReasignarTecnico(int id, [FromBody] ReasignarTecnicoDto dto)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador) return StatusCode(StatusCodes.Status403Forbidden);

        var resultado = await _ticketService.ReasignarTecnicoAsync(id, dto.NuevoTecnicoId, userId!.Value, dto.Motivo);
        if (!resultado) return NotFound();

        return Ok(new { mensaje = "Técnico reasignado exitosamente." });
    }

    [HttpPost("{id}/cambiar-prioridad")]
    public async Task<IActionResult> CambiarPrioridad(int id, [FromBody] CambiarPrioridadDto dto)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador) return StatusCode(StatusCodes.Status403Forbidden);

        var resultado = await _ticketService.ActualizarPrioridadAsync(id, dto.NuevaPrioridad, userId!.Value);
        if (!resultado) return NotFound();

        return Ok(new { mensaje = "Prioridad actualizada correctamente." });
    }

    [HttpPost("{id}/calificar")]
    public async Task<IActionResult> CalificarTicket(int id, [FromBody] CalificarTicketDto dto)
    {
        var (userId, _, _) = ObtenerSesionUsuario();
        if (userId == null) return Unauthorized();

        var resultado = await _ticketService.CalificarTicketAsync(id, dto.Estrellas, dto.Comentario, userId.Value);
        if (!resultado) return StatusCode(StatusCodes.Status403Forbidden);

        return Ok(new { mensaje = "Ticket calificado correctamente." });
    }

    [HttpPost("{id}/notas")]
    public async Task<IActionResult> AgregarNota(int id, [FromBody] AgregarNotaDto dto)
    {
        var (userId, _, _) = ObtenerSesionUsuario();
        if (userId == null) return Unauthorized();

        var resultado = await _ticketService.AgregarNotaAsync(id, userId.Value, dto.Mensaje);
        if (!resultado) return BadRequest(new { mensaje = "No se pudo agregar la nota." });

        return Ok(new { mensaje = "Nota agregada correctamente." });
    }

    [HttpGet("exportar-csv")]
    public async Task<IActionResult> ExportarCsv(
        [FromQuery] string? busqueda = null,
        [FromQuery] EstadoTicket? estado = null,
        [FromQuery] PrioridadTicket? prioridad = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] int? ubicacionId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] string? orden = null)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador) return StatusCode(StatusCodes.Status403Forbidden);

        var pagedResult = await _ticketService.ObtenerTicketsPaginadosAsync(
            busqueda, estado, prioridad, categoriaId, ubicacionId, fechaInicio, fechaFin, pagina: 1, tamanoPagina: 100000, orden);

        var tickets = pagedResult.Items;
        var builder = new StringBuilder();
        builder.AppendLine("sep=;");
        builder.AppendLine("Folio;Asunto / Problema;Categoría;Ubicación;Detalle de Aula;Nivel Prioridad;Estado Actual;Usuario Solicitante;Correo Institucional;Técnico Asignado;Fecha de Reporte;Fecha de Resolución;Calificación del Servicio;Comentario del Usuario");

        foreach (var t in tickets)
        {
            var tituloEscapado = $"\"{t.Titulo.Replace("\"", "\"\"")}\"";
            var detalleAula = !string.IsNullOrEmpty(t.DetalleAula) ? $"\"{t.DetalleAula.Replace("\"", "\"\"")}\"" : "N/A";
            var solicitante = $"\"{t.Solicitante?.NombreCompleto.Replace("\"", "\"\"")}\"";
            var emailSol = t.Solicitante?.Email ?? "N/A";
            var tecnico = t.TecnicoAsignado != null ? $"\"{t.TecnicoAsignado.NombreCompleto.Replace("\"", "\"\"")}\"" : "Sin Asignar";
            var calif = t.CalificacionSatisfaccion.HasValue ? $"{t.CalificacionSatisfaccion} de 5 Estrellas" : "Sin Evaluar";
            var comentario = !string.IsNullOrEmpty(t.ComentarioSatisfaccion) ? $"\"{t.ComentarioSatisfaccion.Replace("\"", "\"\"")}\"" : "Sin comentarios";
            var fechaReporte = t.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            var fechaResolucion = t.FechaResolucion.HasValue ? t.FechaResolucion.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "Pendiente";

            builder.AppendLine($"{t.Id};{tituloEscapado};{t.Categoria?.Nombre};{t.Ubicacion?.Nombre};{detalleAula};{t.Prioridad};{t.Estado};{solicitante};{emailSol};{tecnico};{fechaReporte};{fechaResolucion};{calif};{comentario}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Reporte_Tickets_TecNM_Filtrado_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}

public class CambiarEstadoDto
{
    public EstadoTicket NuevoEstado { get; set; }
    public string? Nota { get; set; }
}

public class ReasignarTecnicoDto
{
    public int? NuevoTecnicoId { get; set; }
    public string? Motivo { get; set; }
}

public class CambiarPrioridadDto
{
    public PrioridadTicket NuevaPrioridad { get; set; }
}

public class CalificarTicketDto
{
    public int Estrellas { get; set; }
    public string? Comentario { get; set; }
}

public class AgregarNotaDto
{
    public string Mensaje { get; set; } = string.Empty;
}
