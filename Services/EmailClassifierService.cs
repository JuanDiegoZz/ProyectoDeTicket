using System.Text.RegularExpressions;
using TicketsApp.Models;

namespace TicketsApp.Services;

public record AnalisisEmailResult(
    bool EsValido,
    TipoSolicitante Tipo,
    PrioridadTicket PrioridadSugerida,
    RolUsuario RolSugerido,
    string MensajeError
);

public interface IEmailClassifierService
{
    AnalisisEmailResult ClasificarEmail(string email);
    bool EsCorreoInstitucionalValido(string email);
}

public class EmailClassifierService : IEmailClassifierService
{
    // Dominio institucional exacto
    private const string DominioInstitucional = "@monclova.tecnm.mx";

    // Regex para Alumnos: 1 letra inicial seguida de 8 dígitos numéricos (Case-Insensitive)
    // Ejemplos: I22050319@monclova.tecnm.mx, G22050319@monclova.tecnm.mx
    private static readonly Regex RegexAlumno = new(
        @"^[a-zA-Z]\d{8}@monclova\.tecnm\.mx$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // Regex para Profesores / Administrativos: nombre.apellido o combinaciones alfanuméricas con punto
    // Ejemplos: ruben.rr@monclova.tecnm.mx, juan.perez@monclova.tecnm.mx
    private static readonly Regex RegexProfesor = new(
        @"^[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+@monclova\.tecnm\.mx$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public AnalisisEmailResult ClasificarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new AnalisisEmailResult(
                EsValido: false,
                Tipo: TipoSolicitante.Desconocido,
                PrioridadSugerida: PrioridadTicket.Normal,
                RolSugerido: RolUsuario.Solicitante,
                MensajeError: "El correo electrónico no puede estar vacío."
            );
        }

        var emailNormalizado = email.Trim().ToLowerInvariant();

        if (!emailNormalizado.EndsWith(DominioInstitucional))
        {
            return new AnalisisEmailResult(
                EsValido: false,
                Tipo: TipoSolicitante.Desconocido,
                PrioridadSugerida: PrioridadTicket.Normal,
                RolSugerido: RolUsuario.Solicitante,
                MensajeError: $"El correo debe pertenecer al dominio institucional '{DominioInstitucional}'."
            );
        }

        // 1. Evaluar si es Alumno
        if (RegexAlumno.IsMatch(emailNormalizado))
        {
            return new AnalisisEmailResult(
                EsValido: true,
                Tipo: TipoSolicitante.Alumno,
                PrioridadSugerida: PrioridadTicket.Normal,
                RolSugerido: RolUsuario.Solicitante,
                MensajeError: string.Empty
            );
        }

        // 2. Evaluar si es Profesor / Personal docente
        if (RegexProfesor.IsMatch(emailNormalizado))
        {
            return new AnalisisEmailResult(
                EsValido: true,
                Tipo: TipoSolicitante.Profesor,
                PrioridadSugerida: PrioridadTicket.Alta,
                RolSugerido: RolUsuario.Solicitante,
                MensajeError: string.Empty
            );
        }

        // 3. Fallback institucional (ej. cuentas generales tipo soporte@, admin@)
        return new AnalisisEmailResult(
            EsValido: true,
            Tipo: TipoSolicitante.Administrativo,
            PrioridadSugerida: PrioridadTicket.Normal,
            RolSugerido: RolUsuario.Solicitante,
            MensajeError: string.Empty
        );
    }

    public bool EsCorreoInstitucionalValido(string email)
    {
        var resultado = ClasificarEmail(email);
        return resultado.EsValido;
    }
}
