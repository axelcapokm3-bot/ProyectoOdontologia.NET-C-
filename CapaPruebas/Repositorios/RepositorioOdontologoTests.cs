namespace ProyectoOdontologia.Tests.Repositorios;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas unitarias del repositorio en memoria <see cref="RepositorioOdontologo"/>.
/// Cubre: caminos felices, validaciones (null / IDs negativos), búsquedas
/// inexistentes y cadenas en blanco. Patron AAA en cada prueba.
/// </summary>
public class RepositorioOdontologoTests
{
    [Fact]
    public async Task ObtenerTodos_RepositorioVacio_RetornaColeccionVacia()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act
        IEnumerable<Odontologo> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerOdontologoPorId_IdExistente_RetornaOdontologo()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        Odontologo odontologo = EntidadesFactory.CrearOdontologo(id: 7);
        await repositorio.AgregarOdontologo(odontologo);

        // Act
        Odontologo? resultado = await repositorio.ObtenerOdontologoPorId(7);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7, resultado.Id);
        Assert.Equal("Dra. Maria Lopez", resultado.Nombre);
    }

    [Fact]
    public async Task ObtenerOdontologoPorId_IdInexistente_RetornaNull()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act
        Odontologo? resultado = await repositorio.ObtenerOdontologoPorId(99);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerOdontologoPorId_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.ObtenerOdontologoPorId(-1));
    }

    [Fact]
    public async Task BuscarOdontologos_ConsultaVacia_RetornaTodosLosOdontologos()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 2));

        // Act
        List<Odontologo> resultado = await repositorio.BuscarOdontologos("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task BuscarOdontologos_ConsultaPorIdNumerico_RetornaOdontologoExacto()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 3));
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 4));

        // Act
        List<Odontologo> resultado = await repositorio.BuscarOdontologos("4");

        // Assert
        Odontologo unico = Assert.Single(resultado);
        Assert.Equal(4, unico.Id);
    }

    [Fact]
    public async Task BuscarOdontologos_ConsultaPorMatricula_RetornaCoincidencias()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));

        // Act
        List<Odontologo> resultado = await repositorio.BuscarOdontologos("mat-1234");

        // Assert
        Odontologo unico = Assert.Single(resultado);
        Assert.Equal("MAT-1234", unico.Matricula);
    }

    [Fact]
    public async Task BuscarOdontologos_ConsultaPorEspecialidad_RetornaCoincidencias()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));

        // Act
        List<Odontologo> resultado = await repositorio.BuscarOdontologos("ortodoncia");

        // Assert
        Odontologo unico = Assert.Single(resultado);
        Assert.Equal("Ortodoncia", unico.Especialidad);
    }

    [Fact]
    public async Task BuscarOdontologos_ConsultaSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));

        // Act
        List<Odontologo> resultado = await repositorio.BuscarOdontologos("cirugia");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AgregarOdontologo_OdontologoValido_AgregaAlRepositorio()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        Odontologo odontologo = EntidadesFactory.CrearOdontologo(id: 5);

        // Act
        await repositorio.AgregarOdontologo(odontologo);

        // Assert
        Assert.NotNull(await repositorio.ObtenerOdontologoPorId(5));
    }

    [Fact]
    public void AgregarOdontologo_OdontologoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.AgregarOdontologo(null!));
    }

    [Fact]
    public async Task ActualizarOdontologo_OdontologoExistente_ActualizaLosValores()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));
        Odontologo odontologoEditado = new Odontologo(1, "Dr. Carlos Ruiz", "MAT-5678", "Endodoncia", 35156789);

        // Act
        await repositorio.ActualizarOdontologo(odontologoEditado);

        // Assert
        Odontologo? resultado = await repositorio.ObtenerOdontologoPorId(1);
        Assert.Equal("Dr. Carlos Ruiz", resultado!.Nombre);
        Assert.Equal("Endodoncia", resultado.Especialidad);
    }

    [Fact]
    public void ActualizarOdontologo_OdontologoInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        Odontologo odontologo = EntidadesFactory.CrearOdontologo(id: 99);

        // Act & Assert
        AssertionHelper.ThrowsSync<KeyNotFoundException>(() => repositorio.ActualizarOdontologo(odontologo));
    }

    [Fact]
    public void ActualizarOdontologo_OdontologoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.ActualizarOdontologo(null!));
    }

    [Fact]
    public async Task EliminarOdontologo_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();
        await repositorio.AgregarOdontologo(EntidadesFactory.CrearOdontologo(id: 1));

        // Act
        bool resultado = await repositorio.EliminarOdontologo(1);

        // Assert
        Assert.True(resultado);
        Assert.Null(await repositorio.ObtenerOdontologoPorId(1));
    }

    [Fact]
    public async Task EliminarOdontologo_IdInexistente_RetornaFalse()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act
        bool resultado = await repositorio.EliminarOdontologo(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EliminarOdontologo_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioOdontologo repositorio = new RepositorioOdontologo();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.EliminarOdontologo(-5));
    }
}