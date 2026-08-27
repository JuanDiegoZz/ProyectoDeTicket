using System.Text.RegularExpressions;
using TicketsApp.Application.Common.Interfaces;
using TicketsApp.Domain.Enums;

namespace TicketsApp.Infrastructure.Services;

public class EmailClassifierService : IEmailClassifierService
{
    private static readonly Regex RegexAlumno = new(
        @"^[a-zA-Z]\d{8}@monclova\.tecnm\.mx$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex RegexProfesor = new(
        @"^[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+@monclova\.tecnm\.mx$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public ClasificacionEmail ClasificarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ClasificacionEmail
            {
                EsValido = false,
                Tipo = TipoSolicitante.Desconocido,
                PrioridadSugerida = PrioridadTicket.Normal,
                MensajeError = "El correo institucional no puede estar vacío."
            };
        }

        var correoLimpio = email.Trim();

        if (RegexAlumno.IsMatch(correoLimpio))
        {
            return new ClasificacionEmail
            {
                EsValido = true,
                Tipo = TipoSolicitante.Alumno,
                PrioridadSugerida = PrioridadTicket.Normal
            };
        }

        if (RegexProfesor.IsMatch(correoLimpio))
        {
            return new ClasificacionEmail
            {
                EsValido = true,
                Tipo = TipoSolicitante.Profesor,
                PrioridadSugerida = PrioridadTicket.Alta
            };
        }

        return new ClasificacionEmail
        {
            EsValido = false,
            Tipo = TipoSolicitante.Desconocido,
            PrioridadSugerida = PrioridadTicket.Normal,
            MensajeError = "El correo debe pertenecer al dominio @monclova.tecnm.mx con formato de Alumno (ej. I22050319@monclova.tecnm.mx) o Docente (ej. nombre.apellido@monclova.tecnm.mx)."
        };
    }
}
