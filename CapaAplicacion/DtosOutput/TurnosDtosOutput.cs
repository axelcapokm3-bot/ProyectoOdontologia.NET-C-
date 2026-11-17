namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

using CapaDominio.Entidades;

public record TurnoDtoOutput(
    int Id,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    int PacienteId,
    int OdontologoId,
    int TratamientoId,
    EstadoTurno Estado
);
