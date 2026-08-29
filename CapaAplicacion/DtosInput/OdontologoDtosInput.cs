namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;

public record OdontologoInputDto
{
    public OdontologoInputDto()
    {
    }

    public OdontologoInputDto(int id, string nombre, string matricula, string especialidad, int telefono)
        : this()
    {
        Id = id;
        Nombre = nombre;
        Matricula = matricula;
        Especialidad = especialidad;
        Telefono = telefono;
    }

    public OdontologoInputDto(int id, string nombre, string matricula, string especialidad, int telefono, int? usuarioId)
        : this(id, nombre, matricula, especialidad, telefono)
    {
        UsuarioId = usuarioId;
    }

    [Required(ErrorMessage = "El ID es obligatorio.")]
    public int Id { get; init; } = default;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "La matricula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La matricula no puede superar los 20 caracteres.")]
    public string Matricula { get; init; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(50, ErrorMessage = "La especialidad no puede superar los 50 caracteres.")]
    public string Especialidad { get; init; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El telefono debe ser un valor numerico valido.")]
    public int Telefono { get; init; } = default;

    public int? UsuarioId { get; init; } = null;
    public string? NombreUsuario { get; init; } = null;
    public string? Password { get; init; } = null;
}
