using CapaAplicacion.Interfaces;
using CapaAplicacion.Seguridad.Interfaces;
using CapaDominio.Entidades;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaAplicacion.Seguridad.Servicios;

public class UsuarioAdminService : IUsuarioAdminService
{
    private readonly IRepositorioUsuario _repositorioUsuario;

    public UsuarioAdminService(IRepositorioUsuario repositorioUsuario)
    {
        _repositorioUsuario = repositorioUsuario;
    }

    public async Task<IReadOnlyCollection<UsuarioDtoOutput>> ObtenerTodosAsync()
    {
        var usuarios = await _repositorioUsuario.ObtenerTodosAsync();
        return usuarios.Select(Mapear).ToList();
    }

    public async Task<UsuarioDtoOutput?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _repositorioUsuario.ObtenerPorIdAsync(id);
        return usuario is null ? null : Mapear(usuario);
    }

    public Task CambiarRolAsync(int id, ActualizarRolUsuarioDto dto)
    {
        return _repositorioUsuario.CambiarRolAsync(id, dto.Rol);
    }

    public Task CambiarEstadoAsync(int id, bool estaActivo)
    {
        return _repositorioUsuario.CambiarEstadoAsync(id, estaActivo);
    }

    private static UsuarioDtoOutput Mapear(Usuario usuario)
    {
        return new UsuarioDtoOutput
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol,
            EstaActivo = usuario.EstaActivo
        };
    }
}
