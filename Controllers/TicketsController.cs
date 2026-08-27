using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Application.ViewModels;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;
using TicketsApp.Infrastructure.Data;

namespace TicketsApp.Controllers;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ITicketService _ticketService;
    private readonly ICatalogoService _catalogoService;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailClassifierService _emailClassifier;

    public TicketsController(
        ApplicationDbContext context,
        ITicketService ticketService,
        ICatalogoService catalogoService,
        IWebHostEnvironment environment,
        IEmailClassifierService emailClassifier)
    {
        _context = context;
        _ticketService = ticketService;
        _catalogoService = catalogoService;
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
    public async Task<IActionResult> Index(
        string? busqueda = null,
        EstadoTicket? estado = null,
        PrioridadTicket? prioridad = null,
        int? categoriaId = null,
        int? ubicacionId = null,
        int pagina = 1,
        int tamanoPagina = 10,
        string? orden = null)
    {
        var (userId, userRol, userEmail) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.UserRol = userRol;
        ViewBag.UserId = userId;

        // 1. ADMINISTRADOR (Dashboard analítico con tabla paginada y filtrada desde servidor)
        if (userRol == RolUsuario.Administrador)
        {
            var pagedTickets = await _ticketService.ObtenerTicketsPaginadosAsync(
                busqueda, estado, prioridad, categoriaId, ubicacionId, pagina, tamanoPagina, orden);

            await CargarCatalogosViewBag();

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

                TicketsPaginados = pagedTickets,
                Busqueda = busqueda,
                EstadoFiltro = estado,
                PrioridadFiltro = prioridad,
                CategoriaFiltro = categoriaId,
                UbicacionFiltro = ubicacionId,
                OrdenActual = orden
            };

            return View("DashboardAdmin", adminVm);
        }

        // 2. TÉCNICO (Tickets asignados o libres)
        if (userRol == RolUsuario.Tecnico)
        {
            var ticketsTecnico = await _ticketService.ObtenerTicketsTecnicoAsync(userId.Value);
            return View("IndexTecnico", ticketsTecnico);
        }

        // 3. SOLICITANTE
        var ticketsSolicitante = await _ticketService.ObtenerTicketsSolicitanteAsync(userId.Value);
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

        var ticket = await _ticketService.ObtenerPorIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        if (userRol == RolUsuario.Solicitante && ticket.SolicitanteId != userId)
        {
            return Forbid();
        }

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

        await _ticketService.CrearTicketAsync(ticket);

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

        var resultado = await _ticketService.CambiarEstadoAsync(ticketId, nuevoEstado, userId.Value, nota);
        if (!resultado) return NotFound();

        TempData["SuccessMessage"] = $"Ticket #{ticketId} actualizado a estado '{nuevoEstado}'.";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReasignarTecnico(int ticketId, int? nuevoTecnicoId, string? motivo)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador)
        {
            return Forbid();
        }

        var resultado = await _ticketService.ReasignarTecnicoAsync(ticketId, nuevoTecnicoId, userId!.Value, motivo);
        if (!resultado) return NotFound();

        TempData["SuccessMessage"] = $"Ticket #{ticketId} reasignado correctamente.";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPrioridad(int ticketId, PrioridadTicket nuevaPrioridad)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador)
        {
            return Forbid();
        }

        var resultado = await _ticketService.ActualizarPrioridadAsync(ticketId, nuevaPrioridad, userId!.Value);
        if (!resultado) return NotFound();

        TempData["SuccessMessage"] = $"Prioridad del Ticket #{ticketId} cambiada a '{nuevaPrioridad}'.";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalificarTicket(int ticketId, int estrellas, string? comentario)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var resultado = await _ticketService.CalificarTicketAsync(ticketId, estrellas, comentario, userId.Value);
        if (!resultado) return Forbid();

        TempData["SuccessMessage"] = "¡Gracias por calificar la atención brindada!";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarNota(int ticketId, string mensaje)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var resultado = await _ticketService.AgregarNotaAsync(ticketId, userId.Value, mensaje);
        if (!resultado)
        {
            TempData["ErrorMessage"] = "No se pudo agregar la nota.";
            return RedirectToAction("Detalle", new { id = ticketId });
        }

        TempData["SuccessMessage"] = "Nota agregada correctamente.";
        return RedirectToAction("Detalle", new { id = ticketId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportarCsv(
        string? busqueda = null,
        EstadoTicket? estado = null,
        PrioridadTicket? prioridad = null,
        int? categoriaId = null,
        int? ubicacionId = null,
        string? orden = null)
    {
        var (userId, userRol, _) = ObtenerSesionUsuario();
        if (userRol != RolUsuario.Administrador)
        {
            return Forbid();
        }

        // Exportación que respeta con exactitud todos los filtros aplicados en la consulta
        var pagedResult = await _ticketService.ObtenerTicketsPaginadosAsync(
            busqueda, estado, prioridad, categoriaId, ubicacionId, pagina: 1, tamanoPagina: 100000, orden);

        var tickets = pagedResult.Items;

        var builder = new StringBuilder();
        // Encabezados en español con codificación UTF-8 BOM para apertura perfecta en Excel
        builder.AppendLine("Folio,Asunto / Problema,Categoría,Ubicación,Detalle de Aula,Nivel Prioridad,Estado Actual,Usuario Solicitante,Correo Institucional,Técnico Asignado,Fecha de Reporte,Fecha de Resolución,Calificación del Servicio,Comentario del Usuario");

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

            builder.AppendLine($"{t.Id},{tituloEscapado},{t.Categoria?.Nombre},{t.Ubicacion?.Nombre},{detalleAula},{t.Prioridad},{t.Estado},{solicitante},{emailSol},{tecnico},{fechaReporte},{fechaResolucion},{calif},{comentario}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"Reporte_Tickets_TecNM_Filtrado_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    private async Task CargarCatalogosViewBag()
    {
        var categorias = await _catalogoService.ObtenerCategoriasAsync(soloActivas: true);
        var ubicaciones = await _catalogoService.ObtenerUbicacionesAsync(soloActivas: true);

        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
        ViewBag.Ubicaciones = new SelectList(ubicaciones, "Id", "Nombre");
    }
}
