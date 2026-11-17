namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using ProyectoOdontologia.Tests.Fakes;
using ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Pruebas de integracion entre la capa de aplicacion y un fake manual del
/// repositorio (ver <see cref="FakeRepositorioInsumos"/>). Simulan el flujo
/// completo servicio -> repositorio en memoria sin infraestructura real.
/// Patron AAA en cada prueba.
/// </summary>
public class InsumosServiceConFakeTests
{
    private readonly FakeRepositorioInsumos _fakeRepositorio;
    private readonly InsumosService _servicio;

    public InsumosServiceConFakeTests()
    {
        _fakeRepositorio = new FakeRepositorioInsumos();
        _servicio = new InsumosService(_fakeRepositorio, new UnitOfWorkNulo());
    }

    [Fact]
    public async Task RegistrarYObtenerTodosAsync_FlujoCompleto_RetornaLosInsumosRegistrados()
    {
        // Arrange
        InsumoDtosInput insumoA = new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);
        InsumoDtosInput insumoB = new InsumoDtosInput(2, "Algodon", CategoriaInsumo.Descartable, 300);

        // Act
        await _servicio.RegistrarInsumoAsync(insumoA);
        await _servicio.RegistrarInsumoAsync(insumoB);
        IEnumerable<InsumoDtosOutput> resultado = await _servicio.ObtenerTodosLosInsumosAsync();

        // Assert
        Assert.Equal(2, resultado.Count());
    }

    [Fact]
    public async Task ActualizarInsumoAsync_InsumoRegistradoPreviamente_ReflejaLosNuevosValores()
    {
        // Arrange
        InsumoDtosInput insumo = new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);
        await _servicio.RegistrarInsumoAsync(insumo);
        InsumoDtosInput insumoEditado = new InsumoDtosInput(1, "Anestesia Modificada", CategoriaInsumo.Anestesia, 800);

        // Act
        await _servicio.ActualizarInsumoAsync(1, insumoEditado);
        InsumoDtosOutput? resultado = await _servicio.ObtenerInsumoPorIdAsync(1);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Anestesia Modificada", resultado.Nombre);
        Assert.Equal(800, resultado.Stock);
    }

    [Fact]
    public async Task ActualizarInsumoAsync_StockMinimoPersonalizado_NoSePierde()
    {
        // Arrange
        FakeRepositorioInsumos repositorio = new FakeRepositorioInsumos();
        InsumosService servicio = new InsumosService(repositorio, new UnitOfWorkNulo());

        await repositorio.AgregarInsumo(
            new Insumo(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500, 0, 100, 50, 80, 300));

        InsumoDtosInput insumoEditado = new InsumoDtosInput(
            1,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            450);

        // Act
        await servicio.ActualizarInsumoAsync(1, insumoEditado);
        Insumo? actualizado = await repositorio.ObtenerInsumoPorId(1);

        // Assert
        Assert.NotNull(actualizado);
        Assert.Equal(450, actualizado!.Stock);
        Assert.Equal(80, actualizado.StockMinimo);
    }

    [Fact]
    public async Task EliminarInsumoAsync_FlujoCompleto_EliminaElRegistro()
    {
        // Arrange
        InsumoDtosInput insumo = new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);
        await _servicio.RegistrarInsumoAsync(insumo);

        // Act
        bool eliminado = await _servicio.EliminarInsumoAsync(1);

        // Assert
        Assert.True(eliminado);
        await Assert.ThrowsAsync<InsumoNoEncontradoException>(
            () => _servicio.EliminarInsumoAsync(1));
    }

    [Fact]
    public async Task BuscarInsumosAsync_FakePoblado_RetornaCoincidenciasPorTexto()
    {
        // Arrange
        await _servicio.RegistrarInsumoAsync(new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));
        await _servicio.RegistrarInsumoAsync(new InsumoDtosInput(2, "Algodon", CategoriaInsumo.Descartable, 300));

        // Act
        List<InsumoDtosOutput> resultado = await _servicio.BuscarInsumosAsync("anestesia");

        // Assert
        InsumoDtosOutput unico = Assert.Single(resultado);
        Assert.Equal("Anestesia Lidocaina", unico.Nombre);
    }
}