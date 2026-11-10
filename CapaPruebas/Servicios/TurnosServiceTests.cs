namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
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
    private readonly Mock<IRepositorioTratamiento> _mockTratamientos;
    private readonly Mock<IRepositorioPaciente> _mockPacientes;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TurnosService _servicio;

    public TurnosServiceTests()
    {
        _mockRepositorio = new Mock<IRepositorioTurnos>();
        _mockTratamientos = new Mock<IRepositorioTratamiento>();
        _mockPacientes = new Mock<IRepositorioPaciente>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno>());
        _mockRepositorio
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _mockRepositorio
            .Setup(r => r.ObtenerTurnosDelDiaAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Turno>());
        _mockRepositorio
            .Setup(r => r.ObtenerProximoIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _mockPacientes
            .Setup(r => r.ObtenerPacientePorId(It.IsAny<int>()))
            .ReturnsAsync(EntidadesFactory.CrearPaciente(id: 1));
        _servicio = new TurnosService(
            _mockRepositorio.Object,
            _mockTratamientos.Object,
            _mockPacientes.Object,
            _mockUnitOfWork.Object);
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
        var metodoImplementacion = typeof(TurnosService).GetMethod(
            nameof(TurnosService.RegistrarTurnoAsync), new[] { typeof(TurnoDtoInput) });
        var metodoInterfaz = typeof(ITurnosService).GetMethod(
            nameof(ITurnosService.RegistrarTurnoAsync), new[] { typeof(TurnoDtoInput) });

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
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servicio.RegistrarTurnoAsync((TurnoDtoInput)null!));
    }

    [Fact]
    public async Task RegistrarTurnoAsync_AgendaSolapadaMismoOdontologo_LanzaTurnoSolapadoYNoPersiste()
    {
        // Arrange
        DateTime baseInicio = DateTime.Now.AddDays(1);
        Turno existente = new Turno(1, baseInicio, baseInicio.AddHours(1), pacienteId: 1, odontologoId: 1, tratamientoId: 1);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Turno> { existente });
        TurnoDtoInput dto = new TurnoDtoInput(baseInicio, baseInicio.AddHours(1), 1, 1, 1);

        // Act & Assert
        await Assert.ThrowsAsync<TurnoSolapadoException>(() => _servicio.RegistrarTurnoAsync(dto));
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
    public async Task RegistrarTurnoAsync_TrasConflictoDeAgenda_PermiteRegistrarElSiguienteTurno()
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

        // Act
        await Assert.ThrowsAsync<TurnoSolapadoException>(() => _servicio.RegistrarTurnoAsync(dtoEnConflicto));
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

    [Fact]
    public async Task RegistrarTurnoAsync_TratamientoConDuracionEstimada_CalculaFechaFinYPersisteDentroDeTransaccion()
    {
        // Arrange
        DateTime inicio = DateTime.Now.AddDays(1).Date.AddHours(9);
        Tratamiento tratamiento = new Tratamiento(5, "Endodoncia", 12000m, new List<TratamientoInsumo>(), TimeSpan.FromMinutes(90));
        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tratamiento);
        _mockRepositorio
            .Setup(r => r.ObtenerProximoIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(77);

        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 3,
            OdontologoId = 2,
            TratamientoId = 5
        };

        // Act
        int id = await _servicio.RegistrarTurnoAsync(dto);

        // Assert
        Assert.Equal(77, id);
        _mockRepositorio.Verify(
            r => r.AgregarTurnoAsync(It.Is<Turno>(t =>
                t.Id == 77 &&
                t.Horario.Inicio == inicio &&
                t.Horario.Fin == inicio.AddMinutes(90) &&
                t.PacienteId == 3 &&
                t.OdontologoId == 2 &&
                t.TratamientoId == 5), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_TratamientoInexistente_LanzaKeyNotFoundExceptionYNoPersiste()
    {
        // Arrange
        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tratamiento?)null);

        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = DateTime.Now.AddDays(1).Date.AddHours(9),
            PacienteId = 1,
            OdontologoId = 1,
            TratamientoId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.RegistrarTurnoAsync(dto));
        _mockRepositorio.Verify(r => r.AgregarTurnoAsync(It.IsAny<Turno>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_PacienteInexistente_LanzaPacienteNoEncontradoYNoPersiste()
    {
        // Arrange
        DateTime inicio = DateTime.Now.AddDays(1).Date.AddHours(9);
        Tratamiento tratamiento = new Tratamiento(1, "Limpieza", 5000m, new List<TratamientoInsumo>(), TimeSpan.FromMinutes(30));

        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tratamiento);
        _mockPacientes
            .Setup(r => r.ObtenerPacientePorId(99))
            .ReturnsAsync((Paciente?)null);

        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 99,
            OdontologoId = 1,
            TratamientoId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<PacienteNoEncontradoException>(() => _servicio.RegistrarTurnoAsync(dto));
        _mockRepositorio.Verify(r => r.AgregarTurnoAsync(It.IsAny<Turno>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarTurnoAsync_SolapamientoEnAgendaDelDia_LanzaTurnoSolapadoYNoConfirma()
    {
        // Arrange
        DateTime inicio = DateTime.Now.AddDays(1).Date.AddHours(9);
        Tratamiento tratamiento = new Tratamiento(1, "Limpieza", 5000m, new List<TratamientoInsumo>(), TimeSpan.FromMinutes(30));
        Turno existente = new Turno(1, inicio.AddMinutes(15), inicio.AddMinutes(45), 1, 1, 1);

        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tratamiento);
        _mockRepositorio
            .Setup(r => r.ObtenerTurnosDelDiaAsync(1, inicio.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Turno> { existente });

        CrearTurnoRequestDto dto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 2,
            OdontologoId = 1,
            TratamientoId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<TurnoSolapadoException>(() => _servicio.RegistrarTurnoAsync(dto));
        _mockRepositorio.Verify(r => r.AgregarTurnoAsync(It.IsAny<Turno>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarHuecosDisponiblesAsync_AgendaVacia_RetornaJornadaLaboralCompleta()
    {
        // Arrange
        DateTime fecha = DateTime.Now.AddDays(1).Date;
        _mockRepositorio
            .Setup(r => r.ObtenerTurnosDelDiaAsync(1, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Turno>());

        ConsultarHuecosRequestDto request = new ConsultarHuecosRequestDto
        {
            OdontologoId = 1,
            Fecha = fecha,
            DuracionMinutos = 60
        };

        // Act
        List<IntervaloTiempoDto> huecos = await _servicio.ConsultarHuecosDisponiblesAsync(request);

        // Assert
        IntervaloTiempoDto unico = Assert.Single(huecos);
        Assert.Equal(fecha.AddHours(8), unico.Inicio);
        Assert.Equal(fecha.AddHours(18), unico.Fin);
    }

    [Fact]
    public async Task CancelarTurnoAsync_TurnoExistente_CancelaYConfirmaTransaccion()
    {
        // Arrange
        Turno turno = EntidadesFactory.CrearTurno(id: 9);
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(turno);

        // Act
        await _servicio.CancelarTurnoAsync(9);

        // Assert
        Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
        _mockRepositorio.Verify(r => r.ActualizarTurnoAsync(turno, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelarTurnoAsync_TurnoInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerTurnoPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Turno?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.CancelarTurnoAsync(123));
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}