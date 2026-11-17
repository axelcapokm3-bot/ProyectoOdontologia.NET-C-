namespace ProyectoOdontologia.Tests.Concurrencia;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using CapaInfraestructura.Persistencia;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using ProyectoOdontologia.Tests.Persistencia;

/// <summary>
/// Pruebas de concurrencia end-to-end (servicio + SQLite real).
/// Verifican que BEGIN IMMEDIATE serialice las escrituras y que la verificación
/// de agenda y la persistencia sean atómicas: con slots no solapados todo se
/// registra, y con el mismo horario solo gana UN turno.
/// </summary>
public class TurnosServiceConcurrenciaTests
{
    private const int CantidadTurnos = 50;

    private static TurnosService CrearServicio(UnitOfWorkSqlite unitOfWork)
    {
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWork);
        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        PacienteRepositorySqlite pacientes = new PacienteRepositorySqlite(unitOfWork);
        OdontologoRepositorySqlite odontologos = new OdontologoRepositorySqlite(unitOfWork);

        return new TurnosService(turnos, tratamientos, pacientes, unitOfWork, null, odontologos);
    }

    private static CrearTurnoRequestDto CrearDto(DateTime inicio)
    {
        return new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 1,
            OdontologoId = 1,
            TratamientoId = 1
        };
    }

    private static async Task InicializarDatosAsync(SqliteTestDatabase baseDeDatos)
    {
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        TratamientoRepositorySqlite tratamientos = new TratamientoRepositorySqlite(unitOfWork);
        await tratamientos.AgregarTratamiento(
            new Tratamiento(
                1,
                "Consulta",
                1000m,
                new List<TratamientoInsumo>(),
                TimeSpan.FromMinutes(30)));

        PacienteRepositorySqlite pacientes = new PacienteRepositorySqlite(unitOfWork);
        await pacientes.AgregarPaciente(
            new Paciente(
                1,
                "Paciente",
                "Concurrente",
                new DateTime(1990, 1, 1),
                "555-0100",
                "concurrente@example.com"));

        OdontologoRepositorySqlite odontologos = new OdontologoRepositorySqlite(unitOfWork);
        await odontologos.AgregarOdontologo(
            new Odontologo(
                1,
                "Odontologo Concurrente",
                "MAT-1234",
                "Odontologia General",
                55500010));
    }

    private static async Task RegistrarTurnoAsync(SqliteTestDatabase baseDeDatos, DateTime inicio)
    {
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        TurnosService servicio = CrearServicio(unitOfWork);
        await servicio.RegistrarTurnoAsync(CrearDto(inicio));
    }

    [Fact]
    public async Task RegistrarTurnoAsync_RegistrosConcurrentesNoSolapados_IdsUnicosYSinPerdida()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await InicializarDatosAsync(baseDeDatos);

        DateTime baseInicio = DateTime.Now.AddDays(1).Date.AddHours(8);
        Task[] tareas = new Task[CantidadTurnos];

        // Act: 50 registros paralelos en slots horarios consecutivos (mismo odontologo).
        for (int i = 0; i < CantidadTurnos; i++)
        {
            DateTime inicio = baseInicio.AddHours(i);
            tareas[i] = RegistrarTurnoAsync(baseDeDatos, inicio);
        }

        await Task.WhenAll(tareas);

        // Assert
        await using UnitOfWorkSqlite unitOfWorkConsulta = baseDeDatos.CrearUnitOfWork();
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWorkConsulta);
        IEnumerable<Turno> resultado = await turnos.ObtenerTodos();
        List<Turno> lista = new List<Turno>(resultado);
        HashSet<int> ids = new HashSet<int>();

        foreach (Turno turno in lista)
        {
            ids.Add(turno.Id);
            Assert.Equal(1, turno.OdontologoId);
        }

        Assert.Equal(CantidadTurnos, lista.Count);
        Assert.Equal(CantidadTurnos, ids.Count);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_RegistrosConcurrentesMismoHorario_UnSoloTurnoGanaYElRestoEsRechazado()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await InicializarDatosAsync(baseDeDatos);

        DateTime inicio = DateTime.Now.AddDays(1).Date.AddHours(8);
        const int intentos = 10;
        Task[] tareas = new Task[intentos];

        // Act: 10 registros identicos en paralelo para el mismo odontologo y horario.
        for (int i = 0; i < intentos; i++)
        {
            tareas[i] = RegistrarTurnoAsync(baseDeDatos, inicio);
        }

        bool lanzoTurnoSolapado = false;

        try
        {
            await Task.WhenAll(tareas);
        }
        catch (TurnoSolapadoException)
        {
            lanzoTurnoSolapado = true;
        }

        // Assert
        Assert.True(lanzoTurnoSolapado);

        await using UnitOfWorkSqlite unitOfWorkConsulta = baseDeDatos.CrearUnitOfWork();
        TurnoRepositorySqlite turnos = new TurnoRepositorySqlite(unitOfWorkConsulta);
        IEnumerable<Turno> resultado = await turnos.ObtenerTodos();
        List<Turno> lista = new List<Turno>(resultado);
        Turno unico = Assert.Single(lista);
        Assert.Equal(1, unico.Id);
    }
}
