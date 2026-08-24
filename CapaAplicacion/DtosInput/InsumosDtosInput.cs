namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;
using CapaDominio.Entidades;

public record InsumoDtosInput
{
    [Required(ErrorMessage = "El ID es obligatorio.")]
    public int Id { get; init; } = default;

    [Required(ErrorMessage = "El nombre del insumo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del insumo no puede superar los 100 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "La categoría del insumo es obligatoria.")]
    [EnumDataType(typeof(CategoriaInsumo), ErrorMessage = "La categoría especificada no es válida.")]
    public CategoriaInsumo Categoria { get; init; } = default;

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser un valor negativo.")]
    public int Stock { get; init; } = default;
}
