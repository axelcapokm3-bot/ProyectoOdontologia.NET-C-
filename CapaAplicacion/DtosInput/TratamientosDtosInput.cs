namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;
using CapaDominio.Entidades;

public record TratamientoDtoInput
{
    [Required(ErrorMessage = "El ID es obligatorio.")]
    public int Id { get; init; } = default;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string Descripcion { get; init; } = string.Empty;

    [Required(ErrorMessage = "El costo es obligatorio.")]
    [Range(0.01, 1000000000, ErrorMessage = "El costo debe ser mayor a 0.")]
    public decimal Costo { get; init; } = default;

    [Range(1, 1440, ErrorMessage = "La duración estimada debe estar entre 1 y 1440 minutos.")]
    public int DuracionMinutos { get; init; } = 30;

    [Required(ErrorMessage = "La lista de insumos requeridos es obligatoria.")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos un insumo requerido.")]
    public List<InsumoRequeridoDtoInput> TratamientoInsumo { get; init; } = new();
}

public record InsumoRequeridoDtoInput
{
    [Required(ErrorMessage = "El ID del insumo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del insumo debe ser mayor a 0.")]
    public int InsumoId { get; init; } = default;

    [Required(ErrorMessage = "La cantidad usada es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad usada debe ser al menos 1.")]
    public int CantidadUsada { get; init; } = default;
}
