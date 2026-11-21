using CapaAplicacion.Interfaces;
using CapaAplicacion.Seguridad.Interfaces;
using CapaDominio.Entidades;
using CapaDominio.Excepciones;
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
        List<UsuarioDtoOutput> resultado = new List<UsuarioDtoOutput>();

        foreach (Usuario usuario in usuarios)
        {
            resultado.Add(Mapear(usuario));
        }

        return resultado;
    }

    public async Task<IReadOnlyCollection<UsuarioDtoOutput>> ListarUsuariosAsync()
    {
        return await ObtenerTodosAsync();
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

    public async Task CambiarRolUsuarioAsync(int usuarioId, RolUsuario nuevoRol)
    {
        Usuario? usuario = await _repositorioUsuario.ObtenerPorIdAsync(usuarioId);

        if (usuario is null)
            throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuarioId}.");

        usuario.AsignarRol(nuevoRol);

        await _repositorioUsuario.ActualizarAsync(usuario);
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
