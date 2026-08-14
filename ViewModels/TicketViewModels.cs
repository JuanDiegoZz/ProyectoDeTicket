using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TicketsApp.Models;

namespace TicketsApp.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo institucional es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [Display(Name = "Nombre Completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo institucional es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
    [Display(Name = "Correo Institucional (@monclova.tecnm.mx)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CrearTicketViewModel
{
    [Required(ErrorMessage = "El asunto o título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede exceder los 150 caracteres.")]
    [Display(Name = "Título del Problema")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe el problema detalladamente.")]
    [Display(Name = "Descripción Detallada")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Selecciona una ubicación del campus.")]
    [Display(Name = "Edificio / Ubicación")]
    public int UbicacionId { get; set; }

    [Display(Name = "Detalle específico (Ej. Aula 204, Cubículo 3)")]
    public string? DetalleAula { get; set; }

    [Display(Name = "Evidencia fotográfica o captura (Opcional)")]
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
    public List<Ticket> UltimosTickets { get; set; } = new();
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
