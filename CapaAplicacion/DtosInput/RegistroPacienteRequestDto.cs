namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;

public class RegistroPacienteRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar los 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime FechaNacimiento { get; set; } = default;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;
}
