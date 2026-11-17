namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;
using Microsoft.Data.Sqlite;

public class SqlitePersistenciaIntegrationTests
{
    [Fact]
    public async Task PersistenciaEntreConexiones_ConservaStockMinimoYMovimiento()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();

        await using (UnitOfWorkSqlite escritura = baseDeDatos.CrearUnitOfWork())
        {
            InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(escritura);

            await repositorio.AgregarInsumo(
                new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 300, 0, 100, 50, 80, 300));

            await repositorio.ActualizarInsumo(
                new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 260, 0, 100, 50, 80, 300));
        }

        // Act
        await using UnitOfWorkSqlite lectura = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorioLectura = new InsumoRepositorySqlite(lectura);
        Insumo? recuperado = await repositorioLectura.ObtenerInsumoPorId(1);

        long movimientos = await lectura.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long tipo = await lectura.Connection.ExecuteScalarAsync<long>(
            "SELECT TipoMovimiento FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long cantidad = await lectura.Connection.ExecuteScalarAsync<long>(
            "SELECT Cantidad FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(260, recuperado!.Stock);
        Assert.Equal(80, recuperado.StockMinimo);
        Assert.Equal(1, movimientos);
        Assert.Equal((int)TipoMovimientoStock.Salida, tipo);
        Assert.Equal(40, cantidad);
    }

    [Fact]
    public async Task MigracionStockMinimo_SobreArchivoPersistente_CompletaValoresYConservaDatos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();

        await using (UnitOfWorkSqlite preparacion = baseDeDatos.CrearUnitOfWork())
        {
            const string esquemaAnterior = @"
CREATE TABLE Insumos (
    Id INTEGER PRIMARY KEY,
    Nombre TEXT NOT NULL,
    Categoria INTEGER NOT NULL,
    Stock INTEGER NOT NULL,
    StockReservado INTEGER NOT NULL,
    PuntoPedido INTEGER NOT NULL,
    StockSeguridad INTEGER NOT NULL,
    StockMaximo INTEGER NOT NULL
);
INSERT INTO Insumos (Id, Nombre, Categoria, Stock, StockReservado, PuntoPedido, StockSeguridad, StockMaximo)
VALUES (1, 'Anestesia Lidocaina', 2, 500, 0, 100, 50, 300);
INSERT INTO Insumos (Id, Nombre, Categoria, Stock, StockReservado, PuntoPedido, StockSeguridad, StockMaximo)
VALUES (2, 'Algodon', 1, 300, 0, 400, 200, 1200);";

            await preparacion.Connection.ExecuteAsync(esquemaAnterior);
        }

        // Act
        await baseDeDatos.InicializarAsync();

        await using (UnitOfWorkSqlite primeraLectura = baseDeDatos.CrearUnitOfWork())
        {
            InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(primeraLectura);
            Insumo? primero = await repositorio.ObtenerInsumoPorId(1);
            Insumo? segundo = await repositorio.ObtenerInsumoPorId(2);

            Assert.NotNull(primero);
            Assert.NotNull(segundo);
            Assert.Equal(100, primero!.StockMinimo);
            Assert.Equal(400, segundo!.StockMinimo);

            await repositorio.ActualizarInsumo(
                new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 450, 0, 100, 50, 100, 300));
        }

        // Assert
        await using UnitOfWorkSqlite segundaLectura = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorioFinal = new InsumoRepositorySqlite(segundaLectura);
        Insumo? recuperado = await repositorioFinal.ObtenerInsumoPorId(1);

        long movimientos = await segundaLectura.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        Assert.NotNull(recuperado);
        Assert.Equal(450, recuperado!.Stock);
        Assert.Equal(100, recuperado.StockMinimo);
        Assert.Equal(1, movimientos);
    }

    [Fact]
    public async Task MigracionTratamientoInsumos_AgregaRestriccionYPreservaDatos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();

        await using (UnitOfWorkSqlite preparacion = baseDeDatos.CrearUnitOfWork())
        {
            const string esquemaAnterior = @"
CREATE TABLE Tratamientos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Descripcion TEXT NOT NULL,
    Costo TEXT NOT NULL,
    DuracionEstimadaTicks INTEGER NOT NULL
);

CREATE TABLE Insumos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Categoria INTEGER NOT NULL,
    Stock INTEGER NOT NULL,
    StockReservado INTEGER NOT NULL,
    PuntoPedido INTEGER NOT NULL,
    StockSeguridad INTEGER NOT NULL,
    StockMaximo INTEGER NOT NULL
);

CREATE TABLE TratamientoInsumos (
    TratamientoId INTEGER NOT NULL,
    InsumoId INTEGER NOT NULL,
    CantidadUsada INTEGER NOT NULL,
    PRIMARY KEY (TratamientoId, InsumoId)
);

INSERT INTO Tratamientos (Id, Descripcion, Costo, DuracionEstimadaTicks)
VALUES (1, 'Limpieza dental', '5000', 18000000000);

INSERT INTO Insumos (Id, Nombre, Categoria, Stock, StockReservado, PuntoPedido, StockSeguridad, StockMaximo)
VALUES (1, 'Anestesia Lidocaina', 2, 100, 0, 100, 50, 300);

INSERT INTO TratamientoInsumos (TratamientoId, InsumoId, CantidadUsada)
VALUES (1, 1, 2);";

            await preparacion.Connection.ExecuteAsync(esquemaAnterior);
        }

        // Act
        await baseDeDatos.InicializarAsync();

        await using UnitOfWorkSqlite lectura = baseDeDatos.CrearUnitOfWork();

        long filas = await lectura.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM TratamientoInsumos;");

        long restricciones = await lectura.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM pragma_foreign_key_list('TratamientoInsumos') WHERE \"table\" = 'Insumos' AND on_delete = 'RESTRICT';");

        // Assert
        Assert.Equal(1, filas);
        Assert.Equal(1, restricciones);

        SqliteException excepcion = await Assert.ThrowsAsync<SqliteException>(() =>
            lectura.Connection.ExecuteAsync("DELETE FROM Insumos WHERE Id = @Id;", new { Id = 1 }));

        Assert.Equal(19, excepcion.SqliteErrorCode);
    }
}
