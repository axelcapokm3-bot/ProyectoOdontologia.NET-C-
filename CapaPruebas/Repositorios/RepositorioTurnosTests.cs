namespace ProyectoOdontologia.Tests.Repositorios;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas unitarias del repositorio en memoria <see cref="RepositorioTurnos"/>.
/// Cubre: caminos felices, validaciones (null / IDs negativos), búsquedas
/// inexistentes y cadenas en blanco. Patron AAA en cada prueba.
/// </summary>
public class RepositorioTurnosTests
{
    [Fact]
    public async Task ObtenerTodos_RepositorioVacio_RetornaColeccionVacia()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act
        IEnumerable<Turno> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerTurnoPorId_IdExistente_RetornaTurno()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        Turno turno = EntidadesFactory.CrearTurno(id: 7);
        await repositorio.AgregarTurno(turno);

        // Act
        Turno? resultado = await repositorio.ObtenerTurnoPorId(7);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7, resultado.Id);
        Assert.Equal(1, resultado.PacienteId);
    }

    [Fact]
    public async Task ObtenerTurnoPorId_IdInexistente_RetornaNull()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act
        Turno? resultado = await repositorio.ObtenerTurnoPorId(99);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerTurnoPorId_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.ObtenerTurnoPorId(-1));
    }

    [Fact]
    public async Task BuscarTurnos_ConsultaVacia_RetornaTodosLosTurnos()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 2));

        // Act
        List<Turno> resultado = await repositorio.BuscarTurnos("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task BuscarTurnos_ConsultaPorIdNumerico_RetornaTurnoExacto()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 3));
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 4));

        // Act
        List<Turno> resultado = await repositorio.BuscarTurnos("4");

        // Assert
        Turno unico = Assert.Single(resultado);
        Assert.Equal(4, unico.Id);
    }

    [Fact]
    public async Task BuscarTurnos_ConsultaPorIdDePaciente_RetornaCoincidencias()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));

        // Act
        List<Turno> resultado = await repositorio.BuscarTurnos("1");

        // Assert
        Assert.NotEmpty(resultado);
    }

    [Fact]
    public async Task BuscarTurnos_ConsultaPorFecha_RetornaCoincidencias()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));

        // Act
        string fecha = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
        List<Turno> resultado = await repositorio.BuscarTurnos(fecha);

        // Assert
        Assert.NotEmpty(resultado);
    }

    [Fact]
    public async Task BuscarTurnos_ConsultaSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));

        // Act
        List<Turno> resultado = await repositorio.BuscarTurnos("zzzzz");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AgregarTurno_TurnoValido_AgregaAlRepositorio()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        Turno turno = EntidadesFactory.CrearTurno(id: 5);

        // Act
        await repositorio.AgregarTurno(turno);

        // Assert
        Assert.NotNull(await repositorio.ObtenerTurnoPorId(5));
    }

    [Fact]
    public void AgregarTurno_TurnoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.AgregarTurno(null!));
    }

    [Fact]
    public async Task ActualizarTurno_TurnoExistente_ActualizaLosValores()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));
        DateTime inicio = DateTime.Now.AddDays(3);
        Turno turnoEditado = new Turno(1, inicio, inicio.AddHours(1), 2, 2, 2);

        // Act
        await repositorio.ActualizarTurno(turnoEditado);

        // Assert
        Turno? resultado = await repositorio.ObtenerTurnoPorId(1);
        Assert.Equal(2, resultado!.PacienteId);
        Assert.Equal(2, resultado.OdontologoId);
    }

    [Fact]
    public void ActualizarTurno_TurnoInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        Turno turno = EntidadesFactory.CrearTurno(id: 99);

        // Act & Assert
        AssertionHelper.ThrowsSync<KeyNotFoundException>(() => repositorio.ActualizarTurno(turno));
    }

    [Fact]
    public void ActualizarTurno_TurnoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.ActualizarTurno(null!));
    }

    [Fact]
    public async Task EliminarTurno_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();
        await repositorio.AgregarTurno(EntidadesFactory.CrearTurno(id: 1));

        // Act
        bool resultado = await repositorio.EliminarTurno(1);

        // Assert
        Assert.True(resultado);
        Assert.Null(await repositorio.ObtenerTurnoPorId(1));
    }

    [Fact]
    public async Task EliminarTurno_IdInexistente_RetornaFalse()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act
        bool resultado = await repositorio.EliminarTurno(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EliminarTurno_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioTurnos repositorio = new RepositorioTurnos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.EliminarTurno(-5));
    }
}