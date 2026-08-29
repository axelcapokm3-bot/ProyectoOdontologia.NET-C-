namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;
using CapaDominio.Entidades;

public class ActualizarRolUsuarioDto
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public RolUsuario Rol { get; set; }
}
