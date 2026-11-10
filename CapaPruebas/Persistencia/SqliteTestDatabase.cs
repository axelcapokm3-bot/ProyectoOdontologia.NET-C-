namespace ProyectoOdontologia.Tests.Persistencia;

using CapaInfraestructura.Persistencia;
using Microsoft.Data.Sqlite;
using global::CapaAplicacion.Interfaces;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly string _rutaArchivo;

    public SqliteTestDatabase()
    {
        _rutaArchivo = Path.Combine(Path.GetTempPath(), $"odontologia-{Guid.NewGuid():N}.db");
        ConnectionString = $"Data Source={_rutaArchivo};Foreign Keys=True";
    }

    public string ConnectionString { get; }

    public IDbConnectionFactory CrearFactory()
    {
        return new SqliteConnectionFactory(ConnectionString);
    }

    public async Task InicializarAsync()
    {
        await DatabaseInitializer.InicializarAsync(CrearFactory());
    }

    public UnitOfWorkSqlite CrearUnitOfWork()
    {
        return new UnitOfWorkSqlite(CrearFactory());
    }

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_rutaArchivo))
        {
            File.Delete(_rutaArchivo);
        }

        return ValueTask.CompletedTask;
    }
}
