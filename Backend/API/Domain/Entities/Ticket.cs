using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Domain.Entities;

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

    [Range(1, 5)]
    [Column("calificacion_satisfaccion")]
    public int? CalificacionSatisfaccion { get; set; }

    [StringLength(300)]
    [Column("comentario_satisfaccion")]
    public string? ComentarioSatisfaccion { get; set; }

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

    public virtual ICollection<NotaTicket> Notas { get; set; } = new List<NotaTicket>();
}
