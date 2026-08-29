using CapaDominio.Entidades;

namespace CapaAplicacion.Interfaces;

public interface IRepositorioUsuario
{
    Task<IReadOnlyCollection<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task AgregarAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task CambiarRolAsync(int usuarioId, RolUsuario rol);
    Task CambiarEstadoAsync(int usuarioId, bool estaActivo);
    Task GuardarRefreshTokenAsync(int usuarioId, RefreshToken refreshToken);
    Task<RefreshToken?> ObtenerRefreshTokenAsync(string token);
    Task RevocarRefreshTokenAsync(string token);
    int IdAutoIncrementoSincrono();
}
