namespace TicketsApp.Domain.Enums;

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
