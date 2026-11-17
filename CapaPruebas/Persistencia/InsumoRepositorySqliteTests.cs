namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;
using Microsoft.Data.Sqlite;

public class InsumoRepositorySqliteTests
{
    [Fact]
    public async Task AgregarInsumo_RehidrataTodosLosCampos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        Insumo insumo = new Insumo(
            1,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            500,
            25,
            100,
            50,
            300);

        // Act
        await repositorio.AgregarInsumo(insumo);
        Insumo? recuperado = await repositorio.ObtenerInsumoPorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(insumo.Id, recuperado!.Id);
        Assert.Equal(insumo.Nombre, recuperado.Nombre);
        Assert.Equal(insumo.Categoria, recuperado.Categoria);
        Assert.Equal(insumo.Stock, recuperado.Stock);
        Assert.Equal(insumo.StockReservado, recuperado.StockReservado);
        Assert.Equal(insumo.StockMinimo, recuperado.StockMinimo);
        Assert.Equal(insumo.PuntoPedido, recuperado.PuntoPedido);
        Assert.Equal(insumo.StockSeguridad, recuperado.StockSeguridad);
        Assert.Equal(insumo.StockMaximo, recuperado.StockMaximo);
    }

    [Fact]
    public async Task AgregarInsumo_PersisteStockMinimoIndependienteDelPuntoPedido()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        Insumo insumo = new Insumo(
            2,
            "Algodon",
            CategoriaInsumo.Descartable,
            300,
            10,
            100,
            50,
            80,
            300);

        // Act
        await repositorio.AgregarInsumo(insumo);
        Insumo? recuperado = await repositorio.ObtenerInsumoPorId(2);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(80, recuperado!.StockMinimo);
        Assert.Equal(100, recuperado.PuntoPedido);
        Assert.False(recuperado.RequiereReabastecimiento());
    }

    [Fact]
    public async Task InicializarAsync_EsquemaAnteriorSinStockMinimo_AgregaLaColumnaYCompletaValores()
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
VALUES (1, 'Anestesia Lidocaina', 2, 500, 0, 100, 50, 300);";

            await preparacion.Connection.ExecuteAsync(esquemaAnterior);
        }

        await baseDeDatos.InicializarAsync();

        // Act
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);
        Insumo? recuperado = await repositorio.ObtenerInsumoPorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(100, recuperado!.StockMinimo);
        Assert.Equal(100, recuperado.PuntoPedido);
    }

    [Fact]
    public async Task ActualizarInsumo_ConCambioDeStock_RegistraMovimientoDeSalida()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        await repositorio.AgregarInsumo(
            new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        Insumo actualizado = new Insumo(
            1,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            450,
            0,
            100,
            50,
            300);

        // Act
        await repositorio.ActualizarInsumo(actualizado);

        long cantidad = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long tipo = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT TipoMovimiento FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long stockAnterior = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT StockAnterior FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long stockNuevo = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT StockNuevo FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        long cantidadMovimiento = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT Cantidad FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        // Assert
        Assert.Equal(1, cantidad);
        Assert.Equal(2, tipo);
        Assert.Equal(500, stockAnterior);
        Assert.Equal(450, stockNuevo);
        Assert.Equal(50, cantidadMovimiento);
        Assert.Equal(450, (await repositorio.ObtenerInsumoPorId(1))!.Stock);
    }

    [Fact]
    public async Task ActualizarInsumo_SinCambioDeStock_NoRegistraMovimiento()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        await repositorio.AgregarInsumo(
            new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        Insumo actualizado = new Insumo(
            1,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            500,
            10,
            100,
            50,
            300);

        // Act
        await repositorio.ActualizarInsumo(actualizado);

        long cantidad = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        // Assert
        Assert.Equal(0, cantidad);
    }

    [Fact]
    public async Task ActualizarInsumo_ConRollback_NoPersisteStockNiMovimiento()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        await repositorio.AgregarInsumo(
            new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        Insumo actualizado = new Insumo(
            1,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            450,
            0,
            100,
            50,
            300);

        await unitOfWork.IniciarTransaccionAsync();

        // Act
        await repositorio.ActualizarInsumo(actualizado);
        await unitOfWork.RollbackAsync();

        long cantidad = await unitOfWork.Connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId;",
            new { InsumoId = 1 });

        // Assert
        Assert.Equal(0, cantidad);
        Assert.Equal(500, (await repositorio.ObtenerInsumoPorId(1))!.Stock);
    }

    [Fact]
    public async Task EliminarInsumo_ConTratamientoAsociado_FallaPorRestriccion()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite insumos = new InsumoRepositorySqlite(unitOfWork);
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);

        await insumos.AgregarInsumo(
            new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        await tratamientos.AgregarTratamiento(
            new Tratamiento(
                1,
                "Limpieza",
                5000m,
                new List<TratamientoInsumo> { new TratamientoInsumo(0, 1, 1) },
                TimeSpan.FromMinutes(30)));

        // Act
        SqliteException excepcion = await Assert.ThrowsAsync<SqliteException>(
            () => insumos.EliminarInsumo(1));

        // Assert
        Assert.Equal(19, excepcion.SqliteErrorCode);
        Assert.NotNull(await insumos.ObtenerInsumoPorId(1));
    }

    [Fact]
    public async Task BuscarInsumos_PorNombreYCategoria_RetornaCoincidencias()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        InsumoRepositorySqlite repositorio = new InsumoRepositorySqlite(unitOfWork);

        await repositorio.AgregarInsumo(new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));
        await repositorio.AgregarInsumo(new Insumo(2, "Algodon", CategoriaInsumo.Descartable, 300));

        // Act
        List<Insumo> porNombre = await repositorio.BuscarInsumos("lidocaina");
        List<Insumo> porCategoria = await repositorio.BuscarInsumos("Descartable");

        // Assert
        Insumo unicoPorNombre = Assert.Single(porNombre);
        Assert.Equal(1, unicoPorNombre.Id);
        Insumo unicoPorCategoria = Assert.Single(porCategoria);
        Assert.Equal(2, unicoPorCategoria.Id);
    }
}
