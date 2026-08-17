namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

using CapaDominio.Entidades;

public record TratamientoDtoOutput(
    int Id,
    string Descripcion,
    decimal Costo,
    List<InsumoRequeridoDtoOutput> listaInsumo
);

public record InsumoRequeridoDtoOutput(
    int IdInsumo,
    int Cantidad
);
