namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using CapaInfraestructura.Persistencia;
using Dapper;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

public class TurnosServiceSqliteTests
{
    private static async Task<TurnosService> CrearServicioAsync(UnitOfWorkSqlite unitOfWork)
    {
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await tratamientos.AgregarTratamiento(
            new Tratamiento(
                1,
                "Consulta",
                1000m,
                new List<TratamientoInsumo>(),
                TimeSpan.FromMinutes(45)));

        PacienteRepositorySqlite pacientes = new PacienteRepositorySqlite(unitOfWork);
        await pacientes.AgregarPaciente(
            new Paciente(
                1,
                "Paciente",
                "Uno",
                new DateTime(1990, 1, 1),
                "555-0001",
                "paciente1@example.com"));
        await pacientes.AgregarPaciente(
            new Paciente(
                2,
                "Paciente",
                "Dos",
                new DateTime(1991, 1, 1),
                "555-0002",
                "paciente2@example.com"));

        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWork);

        OdontologoRepositorySqlite odontologos = new OdontologoRepositorySqlite(unitOfWork);
        await odontologos.AgregarOdontologo(
            new Odontologo(
                1,
                "Odontologo Uno",
                "MAT-1234",
                "Odontologia General",
                55500012));

        return new TurnosService(turnos, tratamientos, pacientes, unitOfWork, null, odontologos);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_CalculaElFinDesdeLaDuracionYPersisteEnSqlite()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TurnosService servicio = await CrearServicioAsync(unitOfWork);
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWork);

        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 1,
            OdontologoId = 1,
            TratamientoId = 1
        };

        // Act
        int id = await servicio.RegistrarTurnoAsync(dto);

        // Assert
        Turno? recuperado = await turnos.ObtenerTurnoPorIdAsync(id);
        Assert.NotNull(recuperado);
        Assert.Equal(inicio, recuperado!.FechaHoraInicio);
        Assert.Equal(inicio.AddMinutes(45), recuperado.FechaHoraFin);
        Assert.Equal(EstadoTurno.Pendiente, recuperado.Estado);
    }

    [Fact]
    public async Task CompletarTurnoAsync_ConsumeInsumosDelTratamientoYPersisteMovimientos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        InsumoRepositorySqlite insumos = new InsumoRepositorySqlite(unitOfWork);
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        PacienteRepositorySqlite pacientes = new PacienteRepositorySqlite(unitOfWork);
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWork);

        await insumos.AgregarInsumo(
            new Insumo(10, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        await tratamientos.AgregarTratamiento(
            new Tratamiento(
                1,
                "Extracción",
                4000m,
                new List<TratamientoInsumo> { new TratamientoInsumo(0, 10, 4) },
                TimeSpan.FromMinutes(45)));

        await pacientes.AgregarPaciente(
            new Paciente(
                1,
                "Paciente",
                "Uno",
                new DateTime(1990, 1, 1),
                "555-0001",
                "paciente1@example.com"));

        TratamientoService tratamientoService = new TratamientoService(tratamientos, insumos, unitOfWork);

        OdontologoRepositorySqlite odontologos = new OdontologoRepositorySqlite(unitOfWork);
        await odontologos.AgregarOdontologo(
            new Odontologo(
                1,
                "Odontologo Uno",
                "MAT-1234",
                "Odontologia General",
                55500012));

        TurnosService servicio = new TurnosService(
            turnos,
            tratamientos,
            pacientes,
            unitOfWork,
            tratamientoService,
            odontologos);

        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = new DateTime(2026, 6, 1, 9, 0, 0),
            PacienteId = 1,
            OdontologoId = 1,
            TratamientoId = 1
        };

        int id = await servicio.RegistrarTurnoAsync(dto);

        // Act
        await servicio.CompletarTurnoAsync(id);

        // Assert
        Turno? turno = await turnos.ObtenerTurnoPorIdAsync(id);
        Assert.NotNull(turno);
        Assert.Equal(EstadoTurno.Completado, turno!.Estado);

        Insumo? insumo = await insumos.ObtenerInsumoPorId(10);
        Assert.NotNull(insumo);
        Assert.Equal(496, insumo!.Stock);

        int movimientos = await unitOfWork.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM MovimientosStock WHERE InsumoId = @InsumoId AND TipoMovimiento = @Tipo;",
            new { InsumoId = 10, Tipo = (int)TipoMovimientoStock.Salida });

        Assert.Equal(1, movimientos);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_Solapamiento_LanzaTurnoSolapadoYNoPersisteElSegundoTurno()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TurnosService servicio = await CrearServicioAsync(unitOfWork);
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWork);

        DateTime inicio = new DateTime(2026, 6, 1, 9, 0, 0);
        CrearTurnoRequestDto primero = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 1,
            OdontologoId = 1,
            TratamientoId = 1
        };

        CrearTurnoRequestDto solapado = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio.AddMinutes(15),
            PacienteId = 2,
            OdontologoId = 1,
            TratamientoId = 1
        };

        // Act
        await servicio.RegistrarTurnoAsync(primero);
        await Assert.ThrowsAsync<TurnoSolapadoException>(() => servicio.RegistrarTurnoAsync(solapado));

        // Assert
        IEnumerable<Turno> todos = await turnos.ObtenerTodos();
        Assert.Single(todos);
    }
}
