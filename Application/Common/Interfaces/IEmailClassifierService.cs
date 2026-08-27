using TicketsApp.Domain.Entities;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Application.Common.Interfaces;

public interface IEmailClassifierService
{
    ClasificacionEmail ClasificarEmail(string email);
}

public class ClasificacionEmail
{
    public bool EsValido { get; set; }
    public TipoSolicitante Tipo { get; set; }
    public PrioridadTicket PrioridadSugerida { get; set; }
    public string MensajeError { get; set; } = string.Empty;
}
