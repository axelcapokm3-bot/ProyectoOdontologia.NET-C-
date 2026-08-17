namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using CapaDominio.Entidades;

public record TratamientoDtoInput(
    string Descripcion,
    decimal Costo,
    List<InsumoRequeridoDtoInput> TratamientoInsumo
);
// NO SABIA QUE SE PUEDE TENER DOS DTO
public record InsumoRequeridoDtoInput(
    int InsumoId,
    int CantidadUsada
);




