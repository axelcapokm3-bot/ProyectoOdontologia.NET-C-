namespace ProyectoOdontologia.Tests.Repositorios;

using CapaDominio.Entidades;
using CapaInfraestructura.Implementacion;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas unitarias del repositorio en memoria <see cref="RepositorioInsumos"/>.
/// Cubre: caminos felices, validaciones (null / IDs negativos), búsquedas
/// inexistentes y cadenas en blanco. Patron AAA en cada prueba.
/// </summary>
public class RepositorioInsumosTests
{
    [Fact]
    public async Task ObtenerTodos_RepositorioVacio_RetornaColeccionVacia()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act
        IEnumerable<Insumo> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerTodos_ConInsumosPreviamenteAgregados_RetornaTodos()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        Insumo insumoA = EntidadesFactory.CrearInsumo(id: 1, nombre: "Anestesia Lidocaina");
        Insumo insumoB = EntidadesFactory.CrearInsumo(id: 2, nombre: "Algodon", stock: 300);
        await repositorio.AgregarInsumo(insumoA);
        await repositorio.AgregarInsumo(insumoB);

        // Act
        IEnumerable<Insumo> resultado = await repositorio.ObtenerTodos();

        // Assert
        Assert.Equal(2, resultado.Count());
        Assert.Contains(resultado, i => i.Id == 1 && i.Nombre == "Anestesia Lidocaina");
        Assert.Contains(resultado, i => i.Id == 2 && i.Nombre == "Algodon");
    }

    [Fact]
    public async Task ObtenerInsumoPorId_IdExistente_RetornaInsumo()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        Insumo insumo = EntidadesFactory.CrearInsumo(id: 7);
        await repositorio.AgregarInsumo(insumo);

        // Act
        Insumo? resultado = await repositorio.ObtenerInsumoPorId(7);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(7, resultado.Id);
        Assert.Equal(insumo.Nombre, resultado.Nombre);
    }

    [Fact]
    public async Task ObtenerInsumoPorId_IdInexistente_RetornaNull()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act
        Insumo? resultado = await repositorio.ObtenerInsumoPorId(99);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerInsumoPorId_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.ObtenerInsumoPorId(-1));
    }

    [Fact]
    public async Task BuscarInsumos_ConsultaVacia_RetornaTodosLosInsumos()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1));
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 2, nombre: "Algodon", stock: 300));

        // Act
        List<Insumo> resultado = await repositorio.BuscarInsumos("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task BuscarInsumos_ConsultaPorIdNumerico_RetornaInsumoExacto()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 3, nombre: "Anestesia Lidocaina"));
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 4, nombre: "Algodon", stock: 300));

        // Act
        List<Insumo> resultado = await repositorio.BuscarInsumos("4");

        // Assert
        Insumo unico = Assert.Single(resultado);
        Assert.Equal(4, unico.Id);
    }

    [Fact]
    public async Task BuscarInsumos_ConsultaPorNombre_RetornaCoincidencias()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1, nombre: "Anestesia Lidocaina"));
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 2, nombre: "Algodon", stock: 300, categoria: CategoriaInsumo.Descartable));

        // Act
        List<Insumo> resultado = await repositorio.BuscarInsumos("anestesia");

        // Assert
        Insumo unico = Assert.Single(resultado);
        Assert.Equal("Anestesia Lidocaina", unico.Nombre);
    }

    [Fact]
    public async Task BuscarInsumos_ConsultaSinCoincidencias_RetornaListaVacia()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1));

        // Act
        List<Insumo> resultado = await repositorio.BuscarInsumos("noexiste");

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task AgregarInsumo_InsumoValido_AgregaAlRepositorio()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        Insumo insumo = EntidadesFactory.CrearInsumo(id: 5);

        // Act
        await repositorio.AgregarInsumo(insumo);

        // Assert
        Assert.NotNull(await repositorio.ObtenerInsumoPorId(5));
    }

    [Fact]
    public void AgregarInsumo_InsumoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.AgregarInsumo(null!));
    }

    [Fact]
    public async Task ActualizarInsumo_InsumoExistente_ActualizaLosValores()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1, nombre: "Anestesia Lidocaina", stock: 500));
        Insumo insumoEditado = EntidadesFactory.CrearInsumo(id: 1, nombre: "Anestesia Modificada", stock: 800);

        // Act
        await repositorio.ActualizarInsumo(insumoEditado);

        // Assert
        Insumo? resultado = await repositorio.ObtenerInsumoPorId(1);
        Assert.Equal("Anestesia Modificada", resultado!.Nombre);
        Assert.Equal(800, resultado.Stock);
    }

    [Fact]
    public void ActualizarInsumo_InsumoInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        Insumo insumo = EntidadesFactory.CrearInsumo(id: 99);

        // Act & Assert
        AssertionHelper.ThrowsSync<KeyNotFoundException>(() => repositorio.ActualizarInsumo(insumo));
    }

    [Fact]
    public void ActualizarInsumo_InsumoNulo_LanzaArgumentNullException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentNullException>(() => repositorio.ActualizarInsumo(null!));
    }

    [Fact]
    public async Task EliminarInsumo_IdExistente_RetornaTrueYLoElimina()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        await repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1));

        // Act
        bool resultado = await repositorio.EliminarInsumo(1);

        // Assert
        Assert.True(resultado);
        Assert.Null(await repositorio.ObtenerInsumoPorId(1));
    }

    [Fact]
    public async Task EliminarInsumo_IdInexistente_RetornaFalse()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act
        bool resultado = await repositorio.EliminarInsumo(999);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EliminarInsumo_IdNegativo_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act & Assert
        AssertionHelper.ThrowsSync<ArgumentOutOfRangeException>(() => repositorio.EliminarInsumo(-5));
    }

    [Fact]
    public async Task AgregarInsumo_AgregadosConcurrentes_IdsUnicos_AgregaTodosSinExcepcion()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        const int cantidad = 200;

        // Act: altas concurrentes sobre el ConcurrentDictionary.
        await Task.WhenAll(
            Enumerable.Range(1, cantidad).Select(i => repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: i))));

        // Assert
        IEnumerable<Insumo> resultado = await repositorio.ObtenerTodos();
        Assert.Equal(cantidad, resultado.Count());
        Assert.Equal(cantidad, resultado.Select(i => i.Id).Distinct().Count());
    }

    [Fact]
    public void AgregarInsumo_IdDuplicado_LanzaArgumentException()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1)).GetAwaiter().GetResult();

        // Act & Assert: conserva el contrato previo de Dictionary.Add sobre clave duplicada.
        AssertionHelper.ThrowsSync<ArgumentException>(() => repositorio.AgregarInsumo(EntidadesFactory.CrearInsumo(id: 1)));
    }
}