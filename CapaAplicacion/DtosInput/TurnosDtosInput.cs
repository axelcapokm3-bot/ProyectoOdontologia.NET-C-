namespace ProyectoOdontologia.CapaAplicacion.DtosInput;



public record TurnoDtoInput(
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    int PacienteId,
    int OdontologoId,
    int TratamientoId
);