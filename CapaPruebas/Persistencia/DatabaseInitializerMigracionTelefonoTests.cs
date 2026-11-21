namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;
using Microsoft.Data.Sqlite;

public class DatabaseInitializerMigracionTelefonoTests
{
    [Fact]
    public async Task InicializarAsync_BaseLegadaConTelefonoEntero_MigraATextoYConservaDatos()
    {
        // Arrange: base con el esquema histórico (Telefono INTEGER) y un celular de 10 dígitos.
        await using SqliteTestDatabase baseDeDatos = new();

        await using (SqliteConnection conexion = new SqliteConnection(baseDeDatos.ConnectionString))
        {
            await conexion.OpenAsync();

            await using (SqliteCommand crearEsquemaLegado = conexion.CreateCommand())
            {
                crearEsquemaLegado.CommandText = @"
CREATE TABLE Odontologos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NULL,
    Nombre TEXT NOT NULL,
    Matricula TEXT NOT NULL,
    Especialidad TEXT NOT NULL,
    Telefono INTEGER NOT NULL
);

INSERT INTO Odontologos (Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono)
VALUES (1, NULL, 'Dra. Legada', 'MAT-0001', 'Odontologia General', 3512345678);";

                await crearEsquemaLegado.ExecuteNonQueryAsync();
            }
        }

        // Act
        await baseDeDatos.InicializarAsync();

        // Assert
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        OdontologoRepositorySqlite repositorio = new OdontologoRepositorySqlite(unitOfWork);

        Odontologo? odontologo = await repositorio.ObtenerOdontologoPorId(1);

        Assert.NotNull(odontologo);
        Assert.Equal("Dra. Legada", odontologo!.Nombre);
        Assert.Equal("3512345678", odontologo.Telefono);

        string? tipoColumna = await unitOfWork.Connection.ExecuteScalarAsync<string>(
            "SELECT type FROM pragma_table_info('Odontologos') WHERE name = 'Telefono';");

        Assert.Equal("TEXT", tipoColumna);
    }

    [Fact]
    public async Task InicializarAsync_BaseYaMigrada_NoVuelveAMigrar()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();

        await using (SqliteConnection conexion = new SqliteConnection(baseDeDatos.ConnectionString))
        {
            await conexion.OpenAsync();

            await using (SqliteCommand insertar = conexion.CreateCommand())
            {
                insertar.CommandText = @"
INSERT INTO Odontologos (UsuarioId, Nombre, Matricula, Especialidad, Telefono)
VALUES (NULL, 'Dra. Texto', 'MAT-0002', 'Ortodoncia', '3518765432');";

                await insertar.ExecuteNonQueryAsync();
            }
        }

        // Act
        await baseDeDatos.InicializarAsync();

        // Assert
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        OdontologoRepositorySqlite repositorio = new OdontologoRepositorySqlite(unitOfWork);

        Odontologo? odontologo = await repositorio.ObtenerOdontologoPorId(1);

        Assert.NotNull(odontologo);
        Assert.Equal("Dra. Texto", odontologo!.Nombre);
        Assert.Equal("3518765432", odontologo.Telefono);
    }
}
