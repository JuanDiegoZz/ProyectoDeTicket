using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Domain.Entities;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [StringLength(150)]
    [Column("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo institucional es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
    [StringLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("rol")]
    public RolUsuario Rol { get; set; } = RolUsuario.Solicitante;

    [Column("tipo_solicitante")]
    public TipoSolicitante TipoSolicitante { get; set; } = TipoSolicitante.Desconocido;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual ICollection<Ticket> TicketsReportados { get; set; } = new List<Ticket>();
    public virtual ICollection<Ticket> TicketsAsignados { get; set; } = new List<Ticket>();
    public virtual ICollection<NotaTicket> Notas { get; set; } = new List<NotaTicket>();
}
