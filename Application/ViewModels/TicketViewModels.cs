using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TicketsApp.Application.Common.Models;
using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Application.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo institucional es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo institucional es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar tu contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CrearTicketViewModel
{
    [Required(ErrorMessage = "El título o asunto del problema es requerido.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción del problema es requerida.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Por favor explica con detalle la falla (mínimo 10 caracteres).")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el tipo de falla / categoría.")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Selecciona la ubicación / edificio.")]
    public int UbicacionId { get; set; }

    [StringLength(150, ErrorMessage = "El detalle de aula no debe superar 150 caracteres.")]
    public string? DetalleAula { get; set; }

    public IFormFile? ArchivoEvidencia { get; set; }
}

public class DashboardAdminViewModel
{
    public int TotalTickets { get; set; }
    public int TicketsAbiertos { get; set; }
    public int TicketsEnProgreso { get; set; }
    public int TicketsResueltos { get; set; }

    public List<MetricaAreaViewModel> FallasPorUbicacion { get; set; } = new();
    public List<MetricaCategoriaViewModel> FallasPorCategoria { get; set; } = new();
    public List<MetricaUsuarioViewModel> TopSolicitantes { get; set; } = new();
    public List<MetricaTecnicoViewModel> EficienciaTecnicos { get; set; } = new();
    
    // Tabla Paginada y Filtrada
    public PagedResult<Ticket> TicketsPaginados { get; set; } = new();

    // Filtros activos
    public string? Busqueda { get; set; }
    public EstadoTicket? EstadoFiltro { get; set; }
    public PrioridadTicket? PrioridadFiltro { get; set; }
    public int? CategoriaFiltro { get; set; }
    public int? UbicacionFiltro { get; set; }
    public string? OrdenActual { get; set; }
}

public class MetricaAreaViewModel
{
    public string Ubicacion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class MetricaCategoriaViewModel
{
    public string Categoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class MetricaUsuarioViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalReportados { get; set; }
}

public class MetricaTecnicoViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public int Resueltos { get; set; }
    public int EnProgreso { get; set; }
}
