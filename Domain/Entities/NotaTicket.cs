using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketsApp.Domain.Entities;

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
