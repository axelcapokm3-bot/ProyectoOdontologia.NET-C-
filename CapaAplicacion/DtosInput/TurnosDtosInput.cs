namespace ProyectoOdontologia.CapaAplicacion.DtosInput;



public record TurnoDtoInput(
    DateTime FechaHora,
    int PacienteId,
    int OdontologoId,
    int TratamientoId
);