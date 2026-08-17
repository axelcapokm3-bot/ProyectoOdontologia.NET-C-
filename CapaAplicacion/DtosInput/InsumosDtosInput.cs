namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using CapaDominio.Entidades;

public record InsumoDtosInput(
    string Nombre,
    CategoriaInsumo Categoria,
    int Stock
);
