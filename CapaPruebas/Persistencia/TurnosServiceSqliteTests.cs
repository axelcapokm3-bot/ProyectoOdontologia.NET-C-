namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using CapaInfraestructura.Persistencia;
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
        return new TurnosService(turnos, tratamientos, pacientes, unitOfWork);
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
