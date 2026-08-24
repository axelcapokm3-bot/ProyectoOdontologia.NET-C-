namespace ProyectoOdontologia.Tests.Repositorios;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas unitarias del repositorio en memoria <see cref="RepositorioPaciente"/>.
/// Cubre: caminos felices, validaciones (null / IDs negativos), búsquedas
/// inexistentes y cadenas en blanco. Patron AAA en cada prueba.
/// </summary>
public class RepositorioPacienteTests
{
    [Fact]
    public async Task ObtenerTodos_RepositorioVacio_RetornaColeccionVacia()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act
        IEnumerable<Paciente> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerPacientePorId_IdExistente_RetornaPaciente()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        Paciente paciente = EntidadesFactory.CrearPaciente(id: 7);
        await repositorio.AgregarPaciente(paciente);

        // Act
        Paciente? resultado = await repositorio.ObtenerPacientePorId(7);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7, resultado.Id);
        Assert.Equal("Juan", resultado.Nombre);
    }

    [Fact]
    public async Task ObtenerPacientePorId_IdInexistente_RetornaNull()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act
        Paciente? resultado = await repositorio.ObtenerPacientePorId(99);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerPacientePorId_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.ObtenerPacientePorId(-1));
    }

    [Fact]
    public async Task BuscarPacientes_ConsultaVacia_RetornaTodosLosPacientes()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 2));

        // Act
        List<Paciente> resultado = await repositorio.BuscarPacientes("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task BuscarPacientes_ConsultaPorIdNumerico_RetornaPacienteExacto()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 3));
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 4));

        // Act
        List<Paciente> resultado = await repositorio.BuscarPacientes("4");

        // Assert
        Paciente unico = Assert.Single(resultado);
        Assert.Equal(4, unico.Id);
    }

    [Fact]
    public async Task BuscarPacientes_ConsultaPorNombre_RetornaCoincidencias()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));
        await repositorio.AgregarPaciente(new Paciente(2, "Maria", "Gomez", new DateTime(1988, 3, 20), "3515551111", "maria@email.com"));

        // Act
        List<Paciente> resultado = await repositorio.BuscarPacientes("juan");

        // Assert
        Paciente unico = Assert.Single(resultado);
        Assert.Equal("Juan", unico.Nombre);
    }

    [Fact]
    public async Task BuscarPacientes_ConsultaPorApellido_RetornaCoincidencias()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));

        // Act
        List<Paciente> resultado = await repositorio.BuscarPacientes("perez");

        // Assert
        Paciente unico = Assert.Single(resultado);
        Assert.Equal("Perez", unico.Apellido);
    }

    [Fact]
    public async Task BuscarPacientes_ConsultaSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));

        // Act
        List<Paciente> resultado = await repositorio.BuscarPacientes("inexistente");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AgregarPaciente_PacienteValido_AgregaAlRepositorio()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        Paciente paciente = EntidadesFactory.CrearPaciente(id: 5);

        // Act
        await repositorio.AgregarPaciente(paciente);

        // Assert
        Assert.NotNull(await repositorio.ObtenerPacientePorId(5));
    }

    [Fact]
    public void AgregarPaciente_PacienteNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.AgregarPaciente(null!));
    }

    [Fact]
    public async Task ActualizarPaciente_PacienteExistente_ActualizaLosValores()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));
        Paciente pacienteEditado = new Paciente(1, "Ana", "Gomez", new DateTime(1992, 1, 10), "3515550000", "ana@email.com");

        // Act
        await repositorio.ActualizarPaciente(pacienteEditado);

        // Assert
        Paciente? resultado = await repositorio.ObtenerPacientePorId(1);
        Assert.Equal("Ana", resultado!.Nombre);
        Assert.Equal("Gomez", resultado.Apellido);
    }

    [Fact]
    public void ActualizarPaciente_PacienteInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        Paciente paciente = EntidadesFactory.CrearPaciente(id: 99);

        // Act & Assert
        AssertionHelper.ThrowsSync<KeyNotFoundException>(() => repositorio.ActualizarPaciente(paciente));
    }

    [Fact]
    public void ActualizarPaciente_PacienteNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.ActualizarPaciente(null!));
    }

    [Fact]
    public async Task EliminarPaciente_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        await repositorio.AgregarPaciente(EntidadesFactory.CrearPaciente(id: 1));

        // Act
        bool resultado = await repositorio.EliminarPaciente(1);

        // Assert
        Assert.True(resultado);
        Assert.Null(await repositorio.ObtenerPacientePorId(1));
    }

    [Fact]
    public async Task EliminarPaciente_IdInexistente_RetornaFalse()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act
        bool resultado = await repositorio.EliminarPaciente(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EliminarPaciente_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.EliminarPaciente(-5));
    }
}