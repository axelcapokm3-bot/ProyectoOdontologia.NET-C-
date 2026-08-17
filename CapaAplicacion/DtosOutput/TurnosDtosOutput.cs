namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;


public record TurnoDtoOutput(
    int Id,
    DateTime FechaHora,
    int PacienteId,
    int OdontologoId,
    int TratamientoId
);
