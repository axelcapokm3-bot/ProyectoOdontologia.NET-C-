namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

public record TratamientoDtoOutput(
    int Id,
    string Descripcion,
    decimal Costo,
    List<InsumoRequeridoDtoOutput> TratamientoInsumo,
    int DuracionMinutos
);

public record InsumoRequeridoDtoOutput(
    int InsumoId,
    int CantidadUsada
);
