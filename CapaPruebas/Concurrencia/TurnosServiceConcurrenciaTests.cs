namespace ProyectoOdontologia.Tests.Concurrencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

/// <summary>
/// Pruebas de concurrencia end-to-end (servicio + repositorio real en memoria).
/// Verifican que la correccion del semaforo (try/finally) garantice que la
/// verificacion de agenda y la persistencia se ejecuten de forma atomica: con
/// slots no solapados todo se registra, y con el mismo horario solo gana UN turno.
/// </summary>
public class TurnosServiceConcurrenciaTests
{
    private const int CantidadTurnos = 50;

    [Fact]
    public async Task RegistrarTurnoAsync_RegistrosConcurrentesNoSolapados_IdsUnicosYSinPerdida()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        TurnosService servicio = new TurnosService(repositorio);
        DateTime baseInicio = DateTime.Now.AddDays(1).Date.AddHours(8);

        // Act: 50 registros paralelos en slots horarios consecutivos (mismo odontologo).
        Task[] tareas = Enumerable.Range(0, CantidadTurnos)
            .Select(i =>
            {
                DateTime inicio = baseInicio.AddHours(i);
                TurnoDtoInput dto = new TurnoDtoInput(inicio, inicio.AddHours(1), PacienteId: 1, OdontologoId: 1, TratamientoId: 1);
                return servicio.RegistrarTurnoAsync(dto);
            })
            .ToArray();

        await Task.WhenAll(tareas);

        // Assert
        IEnumerable<Turno> turnos = await repositorio.ObtenerTodos();
        Assert.Equal(CantidadTurnos, turnos.Count());
        Assert.Equal(CantidadTurnos, turnos.Select(t => t.Id).Distinct().Count());
        Assert.All(turnos, t => Assert.Equal(1, t.OdontologoId));
    }

    [Fact]
    public async Task RegistrarTurnoAsync_RegistrosConcurrentesMismoHorario_UnSoloTurnoGanaYElRestoEsRechazado()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        TurnosService servicio = new TurnosService(repositorio);
        DateTime inicio = DateTime.Now.AddDays(1).Date.AddHours(8);
        const int intentos = 10;

        // Act: 10 registros identicos en paralelo para el mismo odontologo y horario.
        Task[] tareas = Enumerable.Range(0, intentos)
            .Select(_ =>
            {
                TurnoDtoInput dto = new TurnoDtoInput(inicio, inicio.AddHours(1), PacienteId: 1, OdontologoId: 1, TratamientoId: 1);
                return servicio.RegistrarTurnoAsync(dto);
            })
            .ToArray();

        // El semaforo debe permitir que exactamente uno gane y los demas fallen.
        await Assert.ThrowsAsync<InvalidOperationException>(() => Task.WhenAll(tareas));

        // Assert
        IEnumerable<Turno> turnos = await repositorio.ObtenerTodos();
        Turno unico = Assert.Single(turnos);
        Assert.Equal(1, unico.Id);
    }
}