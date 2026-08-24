namespace ProyectoOdontologia.Tests.Repositorios;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas unitarias del repositorio en memoria <see cref="RepositorioTratamiento"/>.
/// Cubre: caminos felices, validaciones (null / IDs negativos), búsquedas
/// inexistentes y cadenas en blanco. Patron AAA en cada prueba.
/// </summary>
public class RepositorioTratamientoTests
{
    [Fact]
    public async Task ObtenerTodos_RepositorioVacio_RetornaColeccionVacia()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act
        IEnumerable<Tratamiento> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerTratamientoPorId_IdExistente_RetornaTratamiento()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        Tratamiento tratamiento = EntidadesFactory.CrearTratamiento(id: 7);
        await repositorio.AgregarTratamiento(tratamiento);

        // Act
        Tratamiento? resultado = await repositorio.ObtenerTratamientoPorId(7);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7, resultado.Id);
        Assert.Equal("Limpieza dental completa", resultado.Descripcion);
    }

    [Fact]
    public async Task ObtenerTratamientoPorId_IdInexistente_RetornaNull()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act
        Tratamiento? resultado = await repositorio.ObtenerTratamientoPorId(99);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerTratamientoPorId_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.ObtenerTratamientoPorId(-1));
    }

    [Fact]
    public async Task BuscarTratamientos_ConsultaVacia_RetornaTodosLosTratamientos()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 1));
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 2));

        // Act
        List<Tratamiento> resultado = await repositorio.BuscarTratamientos("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task BuscarTratamientos_ConsultaPorIdNumerico_RetornaTratamientoExacto()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 3));
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 4));

        // Act
        List<Tratamiento> resultado = await repositorio.BuscarTratamientos("4");

        // Assert
        Tratamiento unico = Assert.Single(resultado);
        Assert.Equal(4, unico.Id);
    }

    [Fact]
    public async Task BuscarTratamientos_ConsultaPorDescripcion_RetornaCoincidencias()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 1));
        await repositorio.AgregarTratamiento(new Tratamiento(2, "Extraccion de muela", 12000m, new List<TratamientoInsumo>()));

        // Act
        List<Tratamiento> resultado = await repositorio.BuscarTratamientos("limpieza");

        // Assert
        Tratamiento unico = Assert.Single(resultado);
        Assert.Equal("Limpieza dental completa", unico.Descripcion);
    }

    [Fact]
    public async Task BuscarTratamientos_ConsultaSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 1));

        // Act
        List<Tratamiento> resultado = await repositorio.BuscarTratamientos("ortodoncia");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AgregarTratamiento_TratamientoValido_AgregaAlRepositorio()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        Tratamiento tratamiento = EntidadesFactory.CrearTratamiento(id: 5);

        // Act
        await repositorio.AgregarTratamiento(tratamiento);

        // Assert
        Assert.NotNull(await repositorio.ObtenerTratamientoPorId(5));
    }

    [Fact]
    public void AgregarTratamiento_TratamientoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.AgregarTratamiento(null!));
    }

    [Fact]
    public async Task ActualizarTratamiento_TratamientoExistente_ActualizaLosValores()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 1));
        Tratamiento tratamientoEditado = new Tratamiento(1, "Extraccion de muela", 12000m, new List<TratamientoInsumo>());

        // Act
        await repositorio.ActualizarTratamiento(tratamientoEditado);

        // Assert
        Tratamiento? resultado = await repositorio.ObtenerTratamientoPorId(1);
        Assert.Equal("Extraccion de muela", resultado!.Descripcion);
        Assert.Equal(12000m, resultado.Costo);
    }

    [Fact]
    public void ActualizarTratamiento_TratamientoInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        Tratamiento tratamiento = EntidadesFactory.CrearTratamiento(id: 99);

        // Act & Assert
        AssertionHelper.ThrowsSync<KeyNotFoundException>(() => repositorio.ActualizarTratamiento(tratamiento));
    }

    [Fact]
    public void ActualizarTratamiento_TratamientoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.ActualizarTratamiento(null!));
    }

    [Fact]
    public async Task EliminarTratamiento_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();
        await repositorio.AgregarTratamiento(EntidadesFactory.CrearTratamiento(id: 1));

        // Act
        bool resultado = await repositorio.EliminarTratamiento(1);

        // Assert
        Assert.True(resultado);
        Assert.Null(await repositorio.ObtenerTratamientoPorId(1));
    }

    [Fact]
    public async Task EliminarTratamiento_IdInexistente_RetornaFalse()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act
        bool resultado = await repositorio.EliminarTratamiento(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EliminarTratamiento_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioTratamiento repositorio = new RepositorioTratamiento();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.EliminarTratamiento(-5));
    }
}