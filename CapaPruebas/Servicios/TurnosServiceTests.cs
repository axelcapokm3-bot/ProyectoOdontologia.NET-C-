namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.ObjetosDeValor;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using ProyectoOdontologia.Tests.Helpers;
using global::CapaAplicacion.Interfaces;

/// <summary>
/// Pruebas de <see cref="TurnosService"/> con Moq sobre <see cref="IRepositorioTurnos"/>.
/// Cubre: firma CQS de registro (Task), asignacion atomica de IDs, validacion de
/// agenda con semaforo (try/finally), metodos CRUD faltantes y el retorno nullable
/// de <see cref="TurnosService.SeleccionarMejorHuecoBestFit"/>. Patron AAA.
/// </summary>
public class TurnosServiceTests
{
    private readonly Mock<IRepositorioTurnos> _mockRepositorio;
    private readonly TurnosService _servicio;

    public TurnosServiceTests()
    {
        _mockRepositorio = new Mock<IRepositorioTurnos>();
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno>());
        _mockRepositorio
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _servicio = new TurnosService(_mockRepositorio.Object);
    }

    private static TurnoDtoInput CrearTurnoDto(int odontologoId = 1, int pacienteId = 1)
    {
        DateTime inicio = DateTime.Now.AddDays(1);
        return new TurnoDtoInput(inicio, inicio.AddHours(1), pacienteId, odontologoId, TratamientoId: 1);
    }

    [Fact]
    public void RegistrarTurnoAsync_FirmaCqs_DevuelveTaskSinDtoDeSalida()
    {
        // Arrange
        var metodoImplementacion = typeof(TurnosService).GetMethod(nameof(TurnosService.RegistrarTurnoAsync));
        var metodoInterfaz = typeof(ITurnosService).GetMethod(nameof(ITurnosService.RegistrarTurnoAsync));

        // Assert: retorno limpio Task (void asincrono), sin DTO de salida.
        Assert.NotNull(metodoImplementacion);
        Assert.NotNull(metodoInterfaz);
        Assert.Equal(typeof(Task), metodoImplementacion.ReturnType);
        Assert.Equal(typeof(Task), metodoInterfaz.ReturnType);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_TurnoValido_UsaIdAutoIncrementoParaInstanciarLaEntidad()
    {
        // Arrange: ID generado distinto a cualquier dato del DTO para detectar mapeos erroneos.
        _mockRepositorio
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(42);
        TurnoDtoInput dto = CrearTurnoDto(odontologoId: 2, pacienteId: 7);

        // Act
        await _servicio.RegistrarTurnoAsync(dto);

        // Assert: el ID del turno proviene del repositorio, NUNCA de PacienteId.
        _mockRepositorio.Verify(
            r => r.AgregarTurno(It.Is<Turno>(t => t.Id == 42 && t.PacienteId == 7 && t.OdontologoId == 2)),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_TurnoValido_PersisteElTurno()
    {
        // Arrange
        TurnoDtoInput dto = CrearTurnoDto();

        // Act
        await _servicio.RegistrarTurnoAsync(dto);

        // Assert
        _mockRepositorio.Verify(r => r.AgregarTurno(It.IsAny<Turno>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_Nulo_LanzaArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servicio.RegistrarTurnoAsync(null!));
    }

    [Fact]
    public async Task RegistrarTurnoAsync_AgendaSolapadaMismoOdontologo_LanzaInvalidOperationExceptionYNoPersiste()
    {
        // Arrange
        DateTime baseInicio = DateTime.Now.AddDays(1);
        Turno existente = new Turno(1, baseInicio, baseInicio.AddHours(1), pacienteId: 1, odontologoId: 1, tratamientoId: 1);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno> { existente });
        TurnoDtoInput dto = new TurnoDtoInput(baseInicio, baseInicio.AddHours(1), 1, 1, 1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.RegistrarTurnoAsync(dto));
        _mockRepositorio.Verify(r => r.AgregarTurno(It.IsAny<Turno>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_AgendaSolapadaDeOtroOdontologo_SiPermiteElRegistro()
    {
        // Arrange: solapamiento temporal pero de otro odontologo => agenda valida.
        DateTime baseInicio = DateTime.Now.AddDays(1);
        Turno existente = new Turno(1, baseInicio, baseInicio.AddHours(1), pacienteId: 1, odontologoId: 1, tratamientoId: 1);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno> { existente });
        TurnoDtoInput dto = new TurnoDtoInput(baseInicio, baseInicio.AddHours(1), 1, OdontologoId: 2, 1);

        // Act
        await _servicio.RegistrarTurnoAsync(dto);

        // Assert
        _mockRepositorio.Verify(r => r.AgregarTurno(It.IsAny<Turno>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_TrasConflictoDeAgenda_ElSemaforoSeLiberaYPermiteRegistrar()
    {
        // Arrange: el 1er llamado a ObtenerTodos devuelve un turno en conflicto, el 2do una agenda libre.
        DateTime baseInicio = DateTime.Now.AddDays(1);
        Turno existente = new Turno(1, baseInicio, baseInicio.AddHours(1), pacienteId: 1, odontologoId: 1, tratamientoId: 1);
        _mockRepositorio
            .SetupSequence(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno> { existente })
            .ReturnsAsync(new List<Turno>());

        TurnoDtoInput dtoEnConflicto = new TurnoDtoInput(baseInicio, baseInicio.AddHours(1), 1, 1, 1);
        TurnoDtoInput dtoValido = new TurnoDtoInput(baseInicio.AddDays(1), baseInicio.AddDays(1).AddHours(1), 1, 1, 1);

        // Act: el 2do registro completaria si el semaforo se libero en el finally.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.RegistrarTurnoAsync(dtoEnConflicto));
        await _servicio.RegistrarTurnoAsync(dtoValido);

        // Assert
        _mockRepositorio.Verify(r => r.AgregarTurno(It.IsAny<Turno>()), Times.Once);
        _mockRepositorio.Verify(r => r.IdAutoIncrementoSincrono(), Times.Exactly(2));
    }

    [Fact]
    public async Task ObtenerTurnoPorIdAsync_IdExistente_RetornaDtoMapeado()
    {
        // Arrange
        Turno turno = EntidadesFactory.CrearTurno(id: 3);
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(3))
            .ReturnsAsync(turno);

        // Act
        TurnoDtoOutput? resultado = await _servicio.ObtenerTurnoPorIdAsync(3);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Id);
        Assert.Equal(turno.PacienteId, resultado.PacienteId);
        Assert.Equal(turno.OdontologoId, resultado.OdontologoId);
    }

    [Fact]
    public async Task ObtenerTurnoPorIdAsync_IdInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(It.IsAny<int>()))
            .ReturnsAsync((Turno?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.ObtenerTurnoPorIdAsync(99));
    }

    [Fact]
    public async Task BuscarTurnosAsync_CriterioCualquiera_RetornaDtosMapeados()
    {
        // Arrange
        List<Turno> turnos = new List<Turno>
        {
            EntidadesFactory.CrearTurno(id: 1),
            EntidadesFactory.CrearTurno(id: 2)
        };
        _mockRepositorio
            .Setup(r => r.BuscarTurnos("1"))
            .ReturnsAsync(turnos);

        // Act
        List<TurnoDtoOutput> resultado = await _servicio.BuscarTurnosAsync("1");

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, dto => Assert.True(dto.Id > 0));
    }

    [Fact]
    public async Task ActualizarTurnoAsync_IdExistente_ActualizaConLaMismaId()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(1))
            .ReturnsAsync(EntidadesFactory.CrearTurno(id: 1));
        TurnoDtoInput dto = CrearTurnoDto(odontologoId: 3, pacienteId: 5);

        // Act
        await _servicio.ActualizarTurnoAsync(1, dto);

        // Assert
        _mockRepositorio.Verify(
            r => r.ActualizarTurno(It.Is<Turno>(t => t.Id == 1 && t.PacienteId == 5 && t.OdontologoId == 3)),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarTurnoAsync_IdInexistente_LanzaKeyNotFoundExceptionYNoActualiza()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(It.IsAny<int>()))
            .ReturnsAsync((Turno?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.ActualizarTurnoAsync(99, CrearTurnoDto()));
        _mockRepositorio.Verify(r => r.ActualizarTurno(It.IsAny<Turno>()), Times.Never);
    }

    [Fact]
    public async Task EliminarTurnoAsync_IdExistente_RetornaTrueYElmina()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(1))
            .ReturnsAsync(EntidadesFactory.CrearTurno(id: 1));
        _mockRepositorio
            .Setup(r => r.EliminarTurno(1))
            .ReturnsAsync(true);

        // Act
        bool resultado = await _servicio.EliminarTurnoAsync(1);

        // Assert
        Assert.True(resultado);
        _mockRepositorio.Verify(r => r.EliminarTurno(1), Times.Once);
    }

    [Fact]
    public async Task EliminarTurnoAsync_IdInexistente_RetornaFalseYNoElmina()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorId(It.IsAny<int>()))
            .ReturnsAsync((Turno?)null);

        // Act
        bool resultado = await _servicio.EliminarTurnoAsync(99);

        // Assert
        Assert.False(resultado);
        _mockRepositorio.Verify(r => r.EliminarTurno(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerHuecosDisponiblesAsync_AgendaVacia_RetornaTodaLaJornadaLaboral()
    {
        // Arrange
        DateTime fecha = DateTime.Now.AddDays(1);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno>());

        // Act
        List<IntervaloDeTiempo> huecos = await _servicio.ObtenerHuecosDisponiblesAsync(1, fecha, TimeSpan.FromHours(1));

        // Assert
        IntervaloDeTiempo unico = Assert.Single(huecos);
        Assert.Equal(fecha.Date.AddHours(8), unico.Inicio);
        Assert.Equal(fecha.Date.AddHours(18), unico.Fin);
    }

    [Fact]
    public void SeleccionarMejorHuecoBestFit_ExisteHuecoExacto_RetornaEseHueco()
    {
        // Arrange
        DateTime inicio = DateTime.Now.AddDays(1);
        IntervaloDeTiempo hueco = new IntervaloDeTiempo(inicio, inicio.AddHours(2));

        // Act
        IntervaloDeTiempo? resultado = _servicio.SeleccionarMejorHuecoBestFit(
            new List<IntervaloDeTiempo> { hueco },
            TimeSpan.FromHours(2));

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(inicio, resultado.Inicio);
        Assert.Equal(inicio.AddHours(2), resultado.Fin);
    }

    [Fact]
    public void SeleccionarMejorHuecoBestFit_VariosHuecos_RetornaElDeMenorResiduo()
    {
        // Arrange: huecoA no alcanza (2h), huecoB residuo 1h, huecoC residuo 3h.
        DateTime inicio = DateTime.Now.AddDays(1);
        IntervaloDeTiempo huecoA = new IntervaloDeTiempo(inicio, inicio.AddHours(2));
        IntervaloDeTiempo huecoB = new IntervaloDeTiempo(inicio.AddDays(1), inicio.AddDays(1).AddHours(4));
        IntervaloDeTiempo huecoC = new IntervaloDeTiempo(inicio.AddDays(2), inicio.AddDays(2).AddHours(6));

        // Act
        IntervaloDeTiempo? resultado = _servicio.SeleccionarMejorHuecoBestFit(
            new List<IntervaloDeTiempo> { huecoA, huecoB, huecoC },
            TimeSpan.FromHours(3));

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(huecoB.Inicio, resultado.Inicio);
        Assert.Equal(huecoB.Inicio.AddHours(3), resultado.Fin);
    }

    [Fact]
    public void SeleccionarMejorHuecoBestFit_NingunHuecoAlcanza_RetornaNull()
    {
        // Arrange
        IntervaloDeTiempo hueco = new IntervaloDeTiempo(DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(1));

        // Act
        IntervaloDeTiempo? resultado = _servicio.SeleccionarMejorHuecoBestFit(
            new List<IntervaloDeTiempo> { hueco },
            TimeSpan.FromHours(3));

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void SeleccionarMejorHuecoBestFit_ListaVacia_RetornaNull()
    {
        // Act
        IntervaloDeTiempo? resultado = _servicio.SeleccionarMejorHuecoBestFit(
            new List<IntervaloDeTiempo>(),
            TimeSpan.FromHours(1));

        // Assert
        Assert.Null(resultado);
    }
}