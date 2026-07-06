namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

public record InsumoDtosOutput(
    Guid Id,
    string Nombre,
    int Stock,
    int PuntoPedido
);