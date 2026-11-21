namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;
using CapaDominio.Entidades;

public class CambiarRolRequestDto
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public RolUsuario Rol { get; set; }
}
