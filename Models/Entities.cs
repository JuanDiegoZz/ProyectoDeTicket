using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketsApp.Models;

public enum RolUsuario
{
    Solicitante,
    Tecnico,
    Administrador
}

public enum TipoSolicitante
{
    Alumno,
    Profesor,
    Administrativo,
    Desconocido
}

public enum PrioridadTicket
{
    Baja,
    Normal,
    Alta,
    Urgente
}

public enum EstadoTicket
{
    Abierto,
    EnProgreso,
    Resuelto,
    Cancelado
}

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

[Table("categorias")]
public class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

[Table("ubicaciones")]
public class Ubicacion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

[Table("tickets")]
public class Ticket
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título o asunto del ticket es requerido.")]
    [StringLength(200)]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción detallada del problema es requerida.")]
    [Column("descripcion", TypeName = "text")]
    public string Descripcion { get; set; } = string.Empty;

    [Column("prioridad")]
    public PrioridadTicket Prioridad { get; set; } = PrioridadTicket.Normal;

    [Column("estado")]
    public EstadoTicket Estado { get; set; } = EstadoTicket.Abierto;

    [StringLength(300)]
    [Column("ruta_evidencia")]
    public string? RutaEvidencia { get; set; }

    [StringLength(150)]
    [Column("detalle_aula")]
    public string? DetalleAula { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("fecha_actualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [Column("fecha_resolucion")]
    public DateTime? FechaResolucion { get; set; }

    // Calificación de satisfacción del usuario (1 a 5 estrellas)
    [Range(1, 5)]
    [Column("calificacion_satisfaccion")]
    public int? CalificacionSatisfaccion { get; set; }

    [StringLength(300)]
    [Column("comentario_satisfaccion")]
    public string? ComentarioSatisfaccion { get; set; }

    // Llaves Foráneas
    [Required]
    [Column("solicitante_id")]
    public int SolicitanteId { get; set; }
    [ForeignKey("SolicitanteId")]
    public virtual Usuario? Solicitante { get; set; }

    [Column("tecnico_asignado_id")]
    public int? TecnicoAsignadoId { get; set; }
    [ForeignKey("TecnicoAsignadoId")]
    public virtual Usuario? TecnicoAsignado { get; set; }

    [Required]
    [Column("categoria_id")]
    public int CategoriaId { get; set; }
    [ForeignKey("CategoriaId")]
    public virtual Categoria? Categoria { get; set; }

    [Required]
    [Column("ubicacion_id")]
    public int UbicacionId { get; set; }
    [ForeignKey("UbicacionId")]
    public virtual Ubicacion? Ubicacion { get; set; }

    // Historial y notas
    public virtual ICollection<NotaTicket> Notas { get; set; } = new List<NotaTicket>();
}

[Table("notas_ticket")]
public class NotaTicket
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("ticket_id")]
    public int TicketId { get; set; }
    [ForeignKey("TicketId")]
    public virtual Ticket? Ticket { get; set; }

    [Required]
    [Column("usuario_id")]
    public int UsuarioId { get; set; }
    [ForeignKey("UsuarioId")]
    public virtual Usuario? Usuario { get; set; }

    [Required]
    [Column("mensaje", TypeName = "text")]
    public string Mensaje { get; set; } = string.Empty;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
