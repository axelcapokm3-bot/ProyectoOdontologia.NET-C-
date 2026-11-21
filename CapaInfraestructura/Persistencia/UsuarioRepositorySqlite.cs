namespace CapaInfraestructura.Persistencia;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class UsuarioRepositorySqlite : IRepositorioUsuario
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private readonly IUnitOfWork _unitOfWork;

    public UsuarioRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<Usuario>> ObtenerTodosAsync()
    {
        const string sql = @"
SELECT Id, NombreUsuario, Email, PasswordHash, Rol, EstaActivo
FROM Usuarios
ORDER BY Id;";

        IEnumerable<UsuarioRow> filas = await _unitOfWork.Connection.QueryAsync<UsuarioRow>(
            new CommandDefinition(sql, transaction: _unitOfWork.Transaction));

        List<Usuario> usuarios = new List<Usuario>();

        foreach (UsuarioRow fila in filas)
        {
            usuarios.Add(Convertir(fila));
        }

        return usuarios;
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return null;

        const string sql = @"
SELECT Id, NombreUsuario, Email, PasswordHash, Rol, EstaActivo
FROM Usuarios
WHERE NombreUsuario = @NombreUsuario
LIMIT 1;";

        UsuarioRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<UsuarioRow>(
            new CommandDefinition(
                sql,
                new { NombreUsuario = nombreUsuario },
                _unitOfWork.Transaction));

        if (fila is null)
            return null;

        return Convertir(fila);
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        const string sql = @"
SELECT Id, NombreUsuario, Email, PasswordHash, Rol, EstaActivo
FROM Usuarios
WHERE Email = @Email
LIMIT 1;";

        UsuarioRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<UsuarioRow>(
            new CommandDefinition(
                sql,
                new { Email = email },
                _unitOfWork.Transaction));

        if (fila is null)
            return null;

        return Convertir(fila);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
SELECT Id, NombreUsuario, Email, PasswordHash, Rol, EstaActivo
FROM Usuarios
WHERE Id = @Id
LIMIT 1;";

        UsuarioRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<UsuarioRow>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        if (fila is null)
            return null;

        return Convertir(fila);
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        if (usuario is null)
            throw new ArgumentNullException(nameof(usuario));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
INSERT INTO Usuarios (Id, NombreUsuario, Email, PasswordHash, Rol, EstaActivo)
VALUES (@Id, @NombreUsuario, @Email, @PasswordHash, @Rol, @EstaActivo);";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(usuario),
                    _unitOfWork.Transaction));

            if (filas != 1)
                throw new InvalidOperationException($"No se pudo insertar el usuario con ID {usuario.Id}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        if (usuario is null)
            throw new ArgumentNullException(nameof(usuario));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
UPDATE Usuarios
SET NombreUsuario = @NombreUsuario,
    Email = @Email,
    PasswordHash = @PasswordHash,
    Rol = @Rol,
    EstaActivo = @EstaActivo
WHERE Id = @Id;";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(usuario),
                    _unitOfWork.Transaction));

            if (filas == 0)
                throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuario.Id}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task CambiarRolAsync(int usuarioId, RolUsuario rol)
    {
        const string sql = @"
UPDATE Usuarios
SET Rol = @Rol
WHERE Id = @UsuarioId;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Rol = (int)rol, UsuarioId = usuarioId },
                _unitOfWork.Transaction));

        if (filas == 0)
            throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuarioId}.");
    }

    public async Task CambiarEstadoAsync(int usuarioId, bool estaActivo)
    {
        const string sql = @"
UPDATE Usuarios
SET EstaActivo = @EstaActivo
WHERE Id = @UsuarioId;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { EstaActivo = estaActivo ? 1 : 0, UsuarioId = usuarioId },
                _unitOfWork.Transaction));

        if (filas == 0)
            throw new KeyNotFoundException($"No se encontro un usuario con el ID {usuarioId}.");
    }

    public async Task GuardarRefreshTokenAsync(int usuarioId, RefreshToken refreshToken)
    {
        if (refreshToken is null)
            throw new ArgumentNullException(nameof(refreshToken));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
INSERT INTO RefreshTokens (UsuarioId, Token, ExpiraUtc, Usado, Revocado)
VALUES (@UsuarioId, @Token, @ExpiraUtc, @Usado, @Revocado);";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        UsuarioId = usuarioId,
                        Token = refreshToken.Token,
                        ExpiraUtc = refreshToken.ExpiracionUtc.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                        Usado = refreshToken.Usado ? 1 : 0,
                        Revocado = refreshToken.Revocado ? 1 : 0
                    },
                    _unitOfWork.Transaction));

            if (filas != 1)
                throw new InvalidOperationException($"No se pudo guardar el refresh token del usuario {usuarioId}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task<RefreshToken?> ObtenerRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        const string sql = @"
SELECT UsuarioId, Token, ExpiraUtc, Usado, Revocado
FROM RefreshTokens
WHERE Token = @Token
ORDER BY Id DESC
LIMIT 1;";

        RefreshTokenRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<RefreshTokenRow>(
            new CommandDefinition(
                sql,
                new { Token = token },
                _unitOfWork.Transaction));

        if (fila is null)
            return null;

        return new RefreshToken(
            fila.Token,
            DateTime.ParseExact(fila.ExpiraUtc, FormatoFecha, CultureInfo.InvariantCulture),
            checked((int)fila.UsuarioId),
            fila.Usado != 0,
            fila.Revocado != 0);
    }

    public async Task RevocarRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        const string sql = @"
UPDATE RefreshTokens
SET Usado = 1,
    Revocado = 1
WHERE Token = @Token;";

        await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Token = token },
                _unitOfWork.Transaction));
    }

    public int IdAutoIncrementoSincrono()
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Usuarios;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    private static Usuario Convertir(UsuarioRow fila)
    {
        return new Usuario(
            checked((int)fila.Id),
            fila.NombreUsuario,
            fila.Email,
            fila.PasswordHash,
            (RolUsuario)fila.Rol,
            fila.EstaActivo != 0);
    }

    private static object CrearParametros(Usuario usuario)
    {
        return new
        {
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Email,
            usuario.PasswordHash,
            Rol = (int)usuario.Rol,
            EstaActivo = usuario.EstaActivo ? 1 : 0
        };
    }
}

internal sealed class UsuarioRow
{
    public long Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Rol { get; set; }
    public long EstaActivo { get; set; }
}

internal sealed class RefreshTokenRow
{
    public long UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string ExpiraUtc { get; set; } = string.Empty;
    public long Usado { get; set; }
    public long Revocado { get; set; }
}
