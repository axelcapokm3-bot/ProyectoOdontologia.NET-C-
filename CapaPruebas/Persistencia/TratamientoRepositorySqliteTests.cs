namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;

public class TratamientoRepositorySqliteTests
{
    [Fact]
    public async Task AgregarTratamiento_TratamientoConInsumos_PersisteDuracionCostoEInsumos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();

        List<TratamientoInsumo> insumos = new List<TratamientoInsumo>
        {
            new TratamientoInsumo(0, 10, 3),
            new TratamientoInsumo(0, 20, 5)
        };

        Tratamiento tratamiento = new Tratamiento(
            1,
            "Endodoncia",
            8000.50m,
            insumos,
            TimeSpan.FromMinutes(75));

        await using (UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork())
        {
            TratamientoRepositorySqlite repositorio = new TratamientoRepositorySqlite(unitOfWork);
            await repositorio.AgregarTratamiento(tratamiento);
        }

        await using UnitOfWorkSqlite verificacion = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite repositorioVerificacion = new TratamientoRepositorySqlite(verificacion);

        // Act
        Tratamiento? recuperado = await repositorioVerificacion.ObtenerTratamientoPorIdAsync(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal("Endodoncia", recuperado!.Descripcion);
        Assert.Equal(8000.50m, recuperado.Costo);
        Assert.Equal(TimeSpan.FromMinutes(75), recuperado.DuracionEstimada);
        Assert.Equal(2, recuperado.InsumosRequeridos.Count);
        Assert.Contains(recuperado.InsumosRequeridos, i => i.InsumoId == 10 && i.CantidadUsada == 3);
        Assert.Contains(recuperado.InsumosRequeridos, i => i.InsumoId == 20 && i.CantidadUsada == 5);
    }

    [Fact]
    public async Task ActualizarTratamiento_ReemplazaLosInsumosExistentes()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite repositorio = new TratamientoRepositorySqlite(unitOfWork);

        Tratamiento original = new Tratamiento(
            1,
            "Limpieza",
            5000m,
            new List<TratamientoInsumo> { new TratamientoInsumo(0, 10, 1) },
            TimeSpan.FromMinutes(30));

        await repositorio.AgregarTratamiento(original);

        Tratamiento modificado = new Tratamiento(
            1,
            "Limpieza profunda",
            6500m,
            new List<TratamientoInsumo> { new TratamientoInsumo(1, 20, 2) },
            TimeSpan.FromMinutes(45));

        // Act
        await repositorio.ActualizarTratamiento(modificado);

        // Assert
        Tratamiento? recuperado = await repositorio.ObtenerTratamientoPorIdAsync(1);
        Assert.NotNull(recuperado);
        Assert.Equal("Limpieza profunda", recuperado!.Descripcion);
        Assert.Equal(6500m, recuperado.Costo);
        Assert.Equal(TimeSpan.FromMinutes(45), recuperado.DuracionEstimada);
        TratamientoInsumo unico = Assert.Single(recuperado.InsumosRequeridos);
        Assert.Equal(20, unico.InsumoId);
        Assert.Equal(2, unico.CantidadUsada);
    }

    [Fact]
    public async Task EliminarTratamiento_EliminaTambienSusInsumos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite repositorio = new TratamientoRepositorySqlite(unitOfWork);

        await repositorio.AgregarTratamiento(
            new Tratamiento(
                1,
                "Extracción",
                4000m,
                new List<TratamientoInsumo> { new TratamientoInsumo(0, 10, 1) },
                TimeSpan.FromMinutes(60)));

        // Act
        bool eliminado = await repositorio.EliminarTratamiento(1);

        int insumosRestantes = await unitOfWork.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM TratamientoInsumos WHERE TratamientoId = @Id;",
            new { Id = 1 });

        // Assert
        Assert.True(eliminado);
        Assert.Equal(0, insumosRestantes);
        Assert.Null(await repositorio.ObtenerTratamientoPorIdAsync(1));
    }

    [Fact]
    public async Task AgregarTratamiento_ConTransaccionExternaActiva_NoHaceCommitAnticipado()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite repositorio = new TratamientoRepositorySqlite(unitOfWork);

        await unitOfWork.IniciarTransaccionAsync();

        // Act
        await repositorio.AgregarTratamiento(
            new Tratamiento(1, "Limpieza", 5000m, new List<TratamientoInsumo>()));

        await unitOfWork.RollbackAsync();

        // Assert
        Assert.Null(await repositorio.ObtenerTratamientoPorIdAsync(1));
    }

    [Fact]
    public async Task BuscarTratamientos_PorIdYPorDescripcion_RetornaCoincidencias()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite repositorio = new TratamientoRepositorySqlite(unitOfWork);

        await repositorio.AgregarTratamiento(
            new Tratamiento(1, "Limpieza dental", 5000m, new List<TratamientoInsumo>()));
        await repositorio.AgregarTratamiento(
            new Tratamiento(2, "Extracción de muela", 12000m, new List<TratamientoInsumo>()));

        // Act
        List<Tratamiento> porId = await repositorio.BuscarTratamientos("2");
        List<Tratamiento> porDescripcion = await repositorio.BuscarTratamientos("limpieza");

        // Assert
        Assert.Single(porId);
        Assert.Equal(2, porId[0].Id);
        Assert.Single(porDescripcion);
        Assert.Equal(1, porDescripcion[0].Id);
    }
}
