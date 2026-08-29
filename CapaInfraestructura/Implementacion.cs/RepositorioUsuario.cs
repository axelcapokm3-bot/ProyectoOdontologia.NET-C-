using System.Collections.Concurrent;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using CapaInfraestructura.Servicios;

namespace CapaInfraestructura.Implementacion;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly ConcurrentDictionary<int, Usuario> _usuarios = new();
    private int _contadorId = 0;

    public RepositorioUsuario()
    {
        SincronizarContadorInicial();

        var hasher = new PasswordHasherBCrypt();
        var admin = new Usuario(
            IdAutoIncrementoSincrono(),
            "admin",
            "admin@local.com",
            hasher.Hash("Admin123!"),
            RolUsuario.Admin);

        _usuarios.TryAdd(admin.Id, admin);
    }

    public Task<IReadOnlyCollection<Usuario>> ObtenerTodosAsync()
    {
        return Task.FromResult((IReadOnlyCollection<Usuario>)_usuarios.Values
            .OrderBy(u => u.Id)
            .ToList());
    }

    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        var usuario = _usuarios.Values.FirstOrDefault(u =>
            u.NombreUsuario.Equals(nombreUsuario, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        var usuario = _usuarios.Values.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        _usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    public Task AgregarAsync(Usuario usuario)
    {
        if (!_usuarios.TryAdd(usuario.Id, usuario))
            throw new ArgumentException($"Ya existe un usuario con el ID {usuario.Id}.");

        return Task.CompletedTask;
    }

    public Task ActualizarAsync(Usuario usuario)
    {
        if (!_usuarios.ContainsKey(usuario.Id))
            throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuario.Id}.");

        _usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }

    public Task CambiarRolAsync(int usuarioId, RolUsuario rol)
    {
        var usuario = _usuarios.GetValueOrDefault(usuarioId)
            ?? throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuarioId}.");

        var actualizado = new Usuario(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Email,
            usuario.PasswordHash,
            rol,
            usuario.EstaActivo);

        foreach (var refreshToken in usuario.RefreshTokens)
        {
            actualizado.AgregarRefreshToken(refreshToken);
        }

        _usuarios[usuarioId] = actualizado;
        return Task.CompletedTask;
    }

    public Task CambiarEstadoAsync(int usuarioId, bool estaActivo)
    {
        var usuario = _usuarios.GetValueOrDefault(usuarioId)
            ?? throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuarioId}.");

        var actualizado = new Usuario(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Email,
            usuario.PasswordHash,
            usuario.Rol,
            estaActivo);

        foreach (var refreshToken in usuario.RefreshTokens)
        {
            actualizado.AgregarRefreshToken(refreshToken);
        }

        _usuarios[usuarioId] = actualizado;
        return Task.CompletedTask;
    }

    public Task GuardarRefreshTokenAsync(int usuarioId, RefreshToken refreshToken)
    {
        var usuario = _usuarios.GetValueOrDefault(usuarioId);
        if (usuario is null) return Task.CompletedTask;

        usuario.AgregarRefreshToken(refreshToken);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> ObtenerRefreshTokenAsync(string token)
    {
        var refreshToken = _usuarios.Values
            .SelectMany(u => u.RefreshTokens)
            .FirstOrDefault(rt => rt.Token == token);

        return Task.FromResult(refreshToken);
    }

    public Task RevocarRefreshTokenAsync(string token)
    {
        var refreshToken = _usuarios.Values
            .SelectMany(u => u.RefreshTokens)
            .FirstOrDefault(rt => rt.Token == token);

        refreshToken?.Revocar();
        return Task.CompletedTask;
    }

    public int IdAutoIncrementoSincrono()
    {
        return Interlocked.Increment(ref _contadorId);
    }

    private void SincronizarContadorInicial()
    {
        _contadorId = _usuarios.Keys.DefaultIfEmpty(0).Max();
    }
}
