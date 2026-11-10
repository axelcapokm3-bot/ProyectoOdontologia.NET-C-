namespace CapaInfraestructura.Persistencia;

using System.Data;
using CapaAplicacion.Interfaces;
using Microsoft.Data.Sqlite;

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La cadena de conexión es obligatoria.", nameof(connectionString));

        SqliteTypeHandlers.AsegurarRegistrados();

        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(connectionString)
        {
            ForeignKeys = true
        };

        _connectionString = builder.ToString();
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
