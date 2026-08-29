using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaAplicacion.Seguridad.Interfaces;

public interface IUsuarioAdminService
{
    Task<IReadOnlyCollection<UsuarioDtoOutput>> ObtenerTodosAsync();
    Task<UsuarioDtoOutput?> ObtenerPorIdAsync(int id);
    Task CambiarRolAsync(int id, ActualizarRolUsuarioDto dto);
    Task CambiarEstadoAsync(int id, bool estaActivo);
}
