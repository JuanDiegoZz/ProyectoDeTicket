using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using TicketsApp.Data;
using TicketsApp.Models;
using TicketsApp.Services;
using TicketsApp.ViewModels;

namespace TicketsApp.Controllers;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailClassifierService _emailClassifier;

    public TicketsController(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        IEmailClassifierService emailClassifier)
    {
        _context = context;
        _environment = environment;
        _emailClassifier = emailClassifier;
    }

    private (int? Id, RolUsuario? Rol, string? Email) ObtenerSesionUsuario()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rolClaim = User.FindFirstValue(ClaimTypes.Role);
        var emailClaim = User.FindFirstValue(ClaimTypes.Email);

        if (int.TryParse(idClaim, out int id) && Enum.TryParse<RolUsuario>(rolClaim, out var rol))
        {
            return (id, rol, emailClaim);
        }
        return (null, null, null);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (userId, userRol, userEmail) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.UserRol = userRol;
        ViewBag.UserId = userId;

        // 1. ADMINISTRADOR (Dashboard y todos los tickets con métricas)
        if (userRol == RolUsuario.Administrador)
        {
            var adminVm = new DashboardAdminViewModel
            {
                TotalTickets = await _context.Tickets.CountAsync(),
                TicketsAbiertos = await _context.Tickets.CountAsync(t => t.Estado == EstadoTicket.Abierto),
                TicketsEnProgreso = await _context.Tickets.CountAsync(t => t.Estado == EstadoTicket.EnProgreso),
                TicketsResueltos = await _context.Tickets.CountAsync(t => t.Estado == EstadoTicket.Resuelto),

                FallasPorUbicacion = await _context.Tickets
                    .GroupBy(t => t.Ubicacion!.Nombre)
                    .Select(g => new MetricaAreaViewModel { Ubicacion = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(5)
                    .ToListAsync(),

                FallasPorCategoria = await _context.Tickets
                    .GroupBy(t => t.Categoria!.Nombre)
                    .Select(g => new MetricaCategoriaViewModel { Categoria = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .ToListAsync(),

                TopSolicitantes = await _context.Tickets
                    .GroupBy(t => new { t.Solicitante!.NombreCompleto, t.Solicitante.Email })
                    .Select(g => new MetricaUsuarioViewModel
                    {
                        Nombre = g.Key.NombreCompleto,
                        Email = g.Key.Email,
                        TotalReportados = g.Count()
                    })
                    .OrderByDescending(x => x.TotalReportados)
                    .Take(5)
                    .ToListAsync(),

                EficienciaTecnicos = await _context.Usuarios
                    .Where(u => u.Rol == RolUsuario.Tecnico && u.Activo)
                    .Select(u => new MetricaTecnicoViewModel
                    {
                        Nombre = u.NombreCompleto,
                        Resueltos = u.TicketsAsignados.Count(t => t.Estado == EstadoTicket.Resuelto),
                        EnProgreso = u.TicketsAsignados.Count(t => t.Estado == EstadoTicket.EnProgreso)
                    })
                    .ToListAsync(),

                UltimosTickets = await _context.Tickets
                    .Include(t => t.Solicitante)
                    .Include(t => t.Categoria)
                    .Include(t => t.Ubicacion)
                    .Include(t => t.TecnicoAsignado)
                    .OrderByDescending(t => t.FechaCreacion)
                    .ToListAsync()
            };

            return View("DashboardAdmin", adminVm);
        }

        // 2. TÉCNICO (Tickets asignados a él o libres sin asignar)
        if (userRol == RolUsuario.Tecnico)
        {
            var ticketsTecnico = await _context.Tickets
                .Include(t => t.Solicitante)
                .Include(t => t.Categoria)
                .Include(t => t.Ubicacion)
                .Include(t => t.TecnicoAsignado)
                .Where(t => t.TecnicoAsignadoId == userId || t.TecnicoAsignadoId == null)
                .OrderByDescending(t => t.Prioridad == PrioridadTicket.Alta)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();

            return View("IndexTecnico", ticketsTecnico);
        }

        // 3. SOLICITANTE
        var ticketsSolicitante = await _context.Tickets
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.TecnicoAsignado)
            .Where(t => t.SolicitanteId == userId)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        return View("IndexSolicitante", ticketsSolicitante);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var ticket = await _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.TecnicoAsignado)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .Include(t => t.Notas)
                .ThenInclude(n => n.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound();
        }

        if (userRol == RolUsuario.Solicitante && ticket.SolicitanteId != userId)
        {
            return Forbid();
        }

        // Cargar lista de técnicos activos para reasignación (solo visible para Administradores)
        if (userRol == RolUsuario.Administrador)
        {
            ViewBag.TecnicosDisponibles = new SelectList(
                await _context.Usuarios.Where(u => u.Rol == RolUsuario.Tecnico && u.Activo).ToListAsync(),
                "Id",
                "NombreCompleto"
            );
        }

        return View(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var (userId, _, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        await CargarCatalogosViewBag();
        return View(new CrearTicketViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearTicketViewModel model)
    {
        var (userId, _, userEmail) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            await CargarCatalogosViewBag();
            return View(model);
        }

        string? rutaGuardada = null;

        if (model.ArchivoEvidencia != null && model.ArchivoEvidencia.Length > 0)
        {
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var extension = Path.GetExtension(model.ArchivoEvidencia.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError("ArchivoEvidencia", "Solo se permiten imágenes (.jpg, .jpeg, .png, .webp) o documentos .pdf");
                await CargarCatalogosViewBag();
                return View(model);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

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

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"¡Ticket #{ticket.Id} generado con éxito! Prioridad asignada: {prioridadAsignada}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int ticketId, EstadoTicket nuevoEstado, string? nota)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null || (userRol != RolUsuario.Tecnico && userRol != RolUsuario.Administrador))
        {
            return Forbid();
        }

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
        {
            return NotFound();
        }

        if (ticket.TecnicoAsignadoId == null && userRol == RolUsuario.Tecnico)
        {
            ticket.TecnicoAsignadoId = userId.Value;
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
                UsuarioId = userId.Value,
                Mensaje = $"[Cambio a {nuevoEstado}]: {nota.Trim()}",
                FechaCreacion = DateTime.UtcNow
            };
            _context.NotasTicket.Add(nuevaNota);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Ticket #{ticketId} actualizado a estado '{nuevoEstado}'.";

        return RedirectToAction("Detalle", new { id = ticketId });
    }

    // POST: /Tickets/ReasignarTecnico (Exclusivo Administrador)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReasignarTecnico(int ticketId, int? nuevoTecnicoId, string? motivo)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador)
        {
            return Forbid();
        }

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
        {
            return NotFound();
        }

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
            UsuarioId = userId!.Value,
            Mensaje = mensajeNota,
            FechaCreacion = DateTime.UtcNow
        };
        _context.NotasTicket.Add(nuevaNota);

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Ticket #{ticketId} reasignado correctamente.";

        return RedirectToAction("Detalle", new { id = ticketId });
    }

    // POST: /Tickets/CalificarTicket (Permite al Solicitante evaluar la atención)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalificarTicket(int ticketId, int estrellas, string? comentario)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null || ticket.SolicitanteId != userId)
        {
            return Forbid();
        }

        ticket.CalificacionSatisfaccion = Math.Clamp(estrellas, 1, 5);
        ticket.ComentarioSatisfaccion = comentario?.Trim();

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "¡Gracias por calificar la atención brindada!";

        return RedirectToAction("Detalle", new { id = ticketId });
    }

    // POST: /Tickets/AgregarNota
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarNota(int ticketId, string mensaje)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return RedirectToAction("Detalle", new { id = ticketId });
        }

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
        {
            return NotFound();
        }

        if (userRol == RolUsuario.Solicitante && ticket.SolicitanteId != userId)
        {
            return Forbid();
        }

        var nuevaNota = new NotaTicket
        {
            TicketId = ticket.Id,
            UsuarioId = userId.Value,
            Mensaje = mensaje.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _context.NotasTicket.Add(nuevaNota);
        ticket.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Nota agregada correctamente.";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    // GET: /Tickets/ExportarCsv (Exportación completa de reportes en Excel/CSV compatible)
    [HttpGet]
    public async Task<IActionResult> ExportarCsv()
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador)
        {
            return Forbid();
        }

        var tickets = await _context.Tickets
            .Include(t => t.Solicitante)
            .Include(t => t.TecnicoAsignado)
            .Include(t => t.Categoria)
            .Include(t => t.Ubicacion)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        var builder = new StringBuilder();
        // Encabezados con BOM para correcta codificación en Microsoft Excel
        builder.AppendLine("Folio,Titulo,Categoria,Ubicacion,Detalle Aula,Prioridad,Estado,Solicitante,Email Solicitante,Tecnico Asignado,Fecha Creacion,Fecha Resolucion,Calificacion");

        foreach (var t in tickets)
        {
            var tituloEscapado = $"\"{t.Titulo.Replace("\"", "\"\"")}\"";
            var solicitante = $"\"{t.Solicitante?.NombreCompleto.Replace("\"", "\"\"")}\"";
            var tecnico = t.TecnicoAsignado != null ? $"\"{t.TecnicoAsignado.NombreCompleto.Replace("\"", "\"\"")}\"" : "Sin Asignar";
            var calif = t.CalificacionSatisfaccion.HasValue ? $"{t.CalificacionSatisfaccion} Estrellas" : "Sin Calificar";
            var fechaRes = t.FechaResolucion.HasValue ? t.FechaResolucion.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "N/A";

            builder.AppendLine($"{t.Id},{tituloEscapado},{t.Categoria?.Nombre},{t.Ubicacion?.Nombre},\"{t.DetalleAula}\",{t.Prioridad},{t.Estado},{solicitante},{t.Solicitante?.Email},{tecnico},{t.FechaCreacion.ToLocalTime():yyyy-MM-dd HH:mm},{fechaRes},{calif}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        return File(bytes, "text/csv", $"Reporte_Tickets_TecNM_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    private async Task CargarCatalogosViewBag()
    {
        ViewBag.Categorias = new SelectList(
            await _context.Categorias.Where(c => c.Activo).ToListAsync(),
            "Id",
            "Nombre"
        );

        ViewBag.Ubicaciones = new SelectList(
            await _context.Ubicaciones.Where(u => u.Activo).ToListAsync(),
            "Id",
            "Nombre"
        );
    }
}
