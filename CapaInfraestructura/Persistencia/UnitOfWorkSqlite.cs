namespace CapaInfraestructura.Persistencia;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using Microsoft.Data.Sqlite;

public sealed class UnitOfWorkSqlite : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private SqliteConnection? _connection;
    private bool _transaccionActiva;

    public UnitOfWorkSqlite(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        SqliteTypeHandlers.AsegurarRegistrados();
    }

    public IDbConnection Connection => _connection ??= AbrirConexion();

    public IDbTransaction? Transaction => null;

    public bool TransaccionActiva => _transaccionActiva;

    public async Task IniciarTransaccionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_transaccionActiva)
            return;

        await EjecutarComandoAsync("BEGIN IMMEDIATE;", cancellationToken);
        _transaccionActiva = true;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        IniciarTransaccionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_transaccionActiva)
            return;

        await EjecutarComandoAsync("COMMIT;", cancellationToken);
        _transaccionActiva = false;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (!_transaccionActiva)
            return;

        await EjecutarComandoAsync("ROLLBACK;", CancellationToken.None);
        _transaccionActiva = false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaccionActiva)
        {
            try
            {
                await EjecutarComandoAsync("ROLLBACK;", CancellationToken.None);
            }
            catch
            {
            }
            finally
            {
                _transaccionActiva = false;
            }
        }

        _connection?.Dispose();
        _connection = null;
    }

    private async Task EjecutarComandoAsync(string sql, CancellationToken cancellationToken)
    {
        SqliteConnection connection = (SqliteConnection)Connection;

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqliteConnection AbrirConexion()
    {
        SqliteConnection connection = (SqliteConnection)_connectionFactory.CreateConnection();
        connection.Open();
        return connection;
    }
}
