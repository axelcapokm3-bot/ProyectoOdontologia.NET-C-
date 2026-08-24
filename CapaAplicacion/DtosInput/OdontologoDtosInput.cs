namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;

public record OdontologoInputDto
{
    [Required(ErrorMessage = "El ID es obligatorio.")]
    public int Id { get; init; } = default;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "La matrícula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La matrícula no puede superar los 20 caracteres.")]
    public string Matricula { get; init; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(50, ErrorMessage = "La especialidad no puede superar los 50 caracteres.")]
    public string Especialidad { get; init; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El teléfono debe ser un valor numérico válido.")]
    public int Telefono { get; init; } = default;
}
