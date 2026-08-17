namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

public record InsumoDtosOutput(
    int Id,
    string Nombre,
    int Stock,
    int StockReserva,
    int PuntoPedido
);
