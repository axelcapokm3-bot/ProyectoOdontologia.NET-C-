namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;


public record TurnoDtoOutput(
    Guid Id,
    DateTime FechaHora,
    int PacienteId,
    int OdontologoId,
    int TratamientoId
);