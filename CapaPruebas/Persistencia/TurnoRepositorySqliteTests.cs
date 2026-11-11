namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;
using Microsoft.Data.Sqlite;

public class TurnoRepositorySqliteTests
{
    private static async Task CrearTratamientoAsync(TratamientoRepositorySqlite repositorio, int id = 1)
    {
        Tratamiento tratamiento = new Tratamiento(
            id,
            "Limpieza dental",
            5000m,
            new List<TratamientoInsumo>(),
            TimeSpan.FromMinutes(45));

        await repositorio.AgregarTratamiento(tratamiento);
    }

    [Fact]
    public async Task Inicializador_CreaTablasEIndiceDeAgenda()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();

        // Act
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        IEnumerable<string> tablas = await unitOfWork.Connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name IN ('Turnos', 'Pacientes', 'Odontologos', 'Tratamientos', 'Insumos', 'TratamientoInsumos', 'MovimientosStock');");

        string? indice = await unitOfWork.Connection.ExecuteScalarAsync<string>(
            "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_Turnos_OdontologoId_FechaHoraInicio';");

        string? indiceMovimientos = await unitOfWork.Connection.ExecuteScalarAsync<string>(
            "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_MovimientosStock_InsumoId_FechaHora';");

        // Assert
        Assert.Contains("Turnos", tablas);
        Assert.Contains("Pacientes", tablas);
        Assert.Contains("Odontologos", tablas);
        Assert.Contains("Tratamientos", tablas);
        Assert.Contains("Insumos", tablas);
        Assert.Contains("TratamientoInsumos", tablas);
        Assert.Contains("MovimientosStock", tablas);
        Assert.Equal("IX_Turnos_OdontologoId_FechaHoraInicio", indice);
        Assert.Equal("IX_MovimientosStock_InsumoId_FechaHora", indiceMovimientos);
    }

    [Fact]
    public async Task AgregarTurnoAsync_TurnoValido_PersisteYRecuperaConEstado()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();

        int id;
        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        DateTime fin = inicio.AddMinutes(45);

        await using (UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork())
        {
            TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
            await CrearTratamientoAsync(tratamientos);

            TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
            id = await repositorio.ObtenerProximoIdAsync();

            Turno turno = new Turno(id, inicio, fin, 1, 1, 1);
            int idInsertado = await repositorio.AgregarTurnoAsync(turno);

            Assert.Equal(id, idInsertado);
        }

        await using UnitOfWorkSqlite verificacion = baseDeDatos.CrearUnitOfWork();
        TurnoRepositorySqlite repositorioVerificacion = new TurnoRepositorySqlite(verificacion);

        // Act
        Turno? recuperado = await repositorioVerificacion.ObtenerTurnoPorIdAsync(id);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(inicio, recuperado!.FechaHoraInicio);
        Assert.Equal(fin, recuperado.FechaHoraFin);
        Assert.Equal(1, recuperado.PacienteId);
        Assert.Equal(1, recuperado.OdontologoId);
        Assert.Equal(1, recuperado.TratamientoId);
        Assert.Equal(EstadoTurno.Pendiente, recuperado.Estado);
    }

    [Fact]
    public async Task ObtenerTurnosDelDiaAsync_FiltraPorOdontologoYFecha()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await CrearTratamientoAsync(tratamientos);

        TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
        DateTime fecha = new DateTime(2026, 6, 1);

        await repositorio.AgregarTurnoAsync(new Turno(await repositorio.ObtenerProximoIdAsync(), fecha.AddHours(8), fecha.AddHours(9), 1, 1, 1));
        await repositorio.AgregarTurnoAsync(new Turno(await repositorio.ObtenerProximoIdAsync(), fecha.AddHours(10), fecha.AddHours(11), 2, 1, 1));
        await repositorio.AgregarTurnoAsync(new Turno(await repositorio.ObtenerProximoIdAsync(), fecha.AddHours(12), fecha.AddHours(13), 1, 2, 1));
        await repositorio.AgregarTurnoAsync(new Turno(await repositorio.ObtenerProximoIdAsync(), fecha.AddDays(1).AddHours(8), fecha.AddDays(1).AddHours(9), 1, 1, 1));

        // Act
        IReadOnlyList<Turno> resultado = await repositorio.ObtenerTurnosDelDiaAsync(1, fecha);

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, t => Assert.Equal(1, t.OdontologoId));
        Assert.Equal(fecha.AddHours(8), resultado[0].FechaHoraInicio);
        Assert.Equal(fecha.AddHours(10), resultado[1].FechaHoraInicio);
    }

    [Fact]
    public async Task ActualizarTurnoAsync_CancelarTurno_PersisteElNuevoEstado()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await CrearTratamientoAsync(tratamientos);

        TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
        int id = await repositorio.ObtenerProximoIdAsync();
        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        Turno turno = new Turno(id, inicio, inicio.AddMinutes(45), 1, 1, 1);
        await repositorio.AgregarTurnoAsync(turno);

        // Act
        turno.Cancelar();
        await repositorio.ActualizarTurnoAsync(turno);

        // Assert
        Turno? recuperado = await repositorio.ObtenerTurnoPorIdAsync(id);
        Assert.NotNull(recuperado);
        Assert.Equal(EstadoTurno.Cancelado, recuperado!.Estado);
    }

    [Fact]
    public async Task BuscarTurnosAsync_PorIdYPorFecha_RetornaCoincidencias()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await CrearTratamientoAsync(tratamientos);

        TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
        int id = await repositorio.ObtenerProximoIdAsync();
        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        await repositorio.AgregarTurnoAsync(new Turno(id, inicio, inicio.AddMinutes(45), 1, 1, 1));

        // Act
        List<Turno> porId = await repositorio.BuscarTurnos(id.ToString());
        List<Turno> porFecha = await repositorio.BuscarTurnos("01/06/2026");

        // Assert
        Assert.Single(porId);
        Assert.Equal(id, porId[0].Id);
        Assert.Single(porFecha);
        Assert.Equal(id, porFecha[0].Id);
    }

    [Fact]
    public async Task IniciarTransaccionAsync_BeginImmediate_BloqueaEscritoresConcurrentes()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();

        await using UnitOfWorkSqlite primero = baseDeDatos.CrearUnitOfWork();
        await primero.IniciarTransaccionAsync();

        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(baseDeDatos.ConnectionString)
        {
            DefaultTimeout = 1
        };

        await using UnitOfWorkSqlite segundo = new UnitOfWorkSqlite(
            new SqliteConnectionFactory(builder.ToString()));

        // Act
        SqliteException excepcion = await Assert.ThrowsAsync<SqliteException>(
            () => segundo.IniciarTransaccionAsync());

        // Assert
        Assert.Equal(5, excepcion.SqliteErrorCode);

        await primero.RollbackAsync();
    }

    [Fact]
    public async Task UnitOfWork_Rollback_NoPersisteElTurno()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await CrearTratamientoAsync(tratamientos);

        TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
        int id = await repositorio.ObtenerProximoIdAsync();
        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);

        // Act
        await unitOfWork.IniciarTransaccionAsync();
        await repositorio.AgregarTurnoAsync(new Turno(id, inicio, inicio.AddMinutes(45), 1, 1, 1));
        await unitOfWork.RollbackAsync();

        // Assert
        Assert.Null(await repositorio.ObtenerTurnoPorIdAsync(id));
    }

    [Fact]
    public async Task EliminarTurnoAsync_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await CrearTratamientoAsync(tratamientos);

        TurnoRepositorySqlite repositorio = new TurnoRepositorySqlite(unitOfWork);
        int id = await repositorio.ObtenerProximoIdAsync();
        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        await repositorio.AgregarTurnoAsync(new Turno(id, inicio, inicio.AddMinutes(45), 1, 1, 1));

        // Act
        bool eliminado = await repositorio.EliminarTurno(id);

        // Assert
        Assert.True(eliminado);
        Assert.Null(await repositorio.ObtenerTurnoPorIdAsync(id));
        Assert.False(await repositorio.EliminarTurno(id));
    }
}
