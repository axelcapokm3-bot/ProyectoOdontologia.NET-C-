namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

public record InsumoDtosOutput(
    int Id,
    string Nombre,
    int Stock,
    int StockReservado,
    int PuntoPedido,
    int StockMinimo
);
