namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

using CapaDominio.Entidades;

public class UsuarioDtoOutput
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool EstaActivo { get; set; }
}
