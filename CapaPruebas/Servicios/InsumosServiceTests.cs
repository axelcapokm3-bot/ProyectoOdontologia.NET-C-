namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using ProyectoOdontologia.Tests.Helpers;
using global::CapaAplicacion.Interfaces;

/// <summary>
/// Pruebas de la capa de aplicacion <see cref="InsumosService"/>.
/// Se usa Moq para simular <see cref="IRepositorioInsumos"/> y validar las
/// interacciones del servicio con la abstraccion, aisladas de la infraestructura.
/// Patron AAA en cada prueba.
/// </summary>
public class InsumosServiceTests
{
    private readonly Mock<IRepositorioInsumos> _mockRepositorio;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly InsumosService _servicio;

    public InsumosServiceTests()
    {
        // Arrange comun: se re-crea el mock y el servicio en cada prueba.
        _mockRepositorio = new Mock<IRepositorioInsumos>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepositorio
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _servicio = new InsumosService(_mockRepositorio.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task RegistrarInsumoAsync_InsumoValido_InvocaAgregarInsumo()
    {
        // Arrange
        InsumoDtosInput insumoDto = new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);

        // Act
        await _servicio.RegistrarInsumoAsync(insumoDto);

        // Assert
        _mockRepositorio.Verify(
            r => r.AgregarInsumo(It.Is<Insumo>(i => i.Nombre == "Anestesia Lidocaina")),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarInsumoAsync_NombreConCaracteresInvalidos_LanzaArgumentException()
    {
        // Arrange
        InsumoDtosInput insumoDto = new InsumoDtosInput(1, "Anestesia 123", CategoriaInsumo.Anestesia, 500);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.RegistrarInsumoAsync(insumoDto));
        _mockRepositorio.Verify(r => r.AgregarInsumo(It.IsAny<Insumo>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerInsumoPorIdAsync_IdExistente_RetornaDtoConDatos()
    {
        // Arrange
        Insumo insumo = EntidadesFactory.CrearInsumo(id: 3);
        _mockRepositorio
            .Setup(r => r.ObtenerInsumoPorId(3))
            .ReturnsAsync(insumo);

        // Act
        InsumoDtosOutput? resultado = await _servicio.ObtenerInsumoPorIdAsync(3);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.Id);
        Assert.Equal(insumo.Nombre, resultado.Nombre);
    }

    [Fact]
    public async Task ObtenerInsumoPorIdAsync_IdInexistente_LanzaInsumoNoEncontradoException()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerInsumoPorId(It.IsAny<int>()))
            .ReturnsAsync((Insumo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InsumoNoEncontradoException>(
            () => _servicio.ObtenerInsumoPorIdAsync(99));
    }

    [Fact]
    public async Task EliminarInsumoAsync_IdExistente_RetornaTrue()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerInsumoPorId(1))
            .ReturnsAsync(EntidadesFactory.CrearInsumo(id: 1));
        _mockRepositorio
            .Setup(r => r.EliminarInsumo(1))
            .ReturnsAsync(true);

        // Act
        bool resultado = await _servicio.EliminarInsumoAsync(1);

        // Assert
        Assert.True(resultado);
        _mockRepositorio.Verify(r => r.EliminarInsumo(1), Times.Once);
    }

    [Fact]
    public async Task EliminarInsumoAsync_IdInexistente_LanzaInsumoNoEncontradoException()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerInsumoPorId(It.IsAny<int>()))
            .ReturnsAsync((Insumo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InsumoNoEncontradoException>(
            () => _servicio.EliminarInsumoAsync(99));
        _mockRepositorio.Verify(r => r.EliminarInsumo(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuscarInsumosAsync_CriterioVacio_RetornaTodos()
    {
        // Arrange
        List<Insumo> insumos = new List<Insumo>
        {
            EntidadesFactory.CrearInsumo(id: 1, nombre: "Anestesia Lidocaina"),
            EntidadesFactory.CrearInsumo(id: 2, nombre: "Algodon", stock: 300)
        };

        _mockRepositorio
            .Setup(r => r.BuscarInsumos(It.IsAny<string>()))
            .ReturnsAsync(insumos);

        // Act
        List<InsumoDtosOutput> resultado = await _servicio.BuscarInsumosAsync("  ");

        // Assert
        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public void ValidadorNombre_NombreNulo_RetornaFalse()
    {
        // Arrange
        InsumoDtosInput insumoDto = new InsumoDtosInput(1, "", CategoriaInsumo.Anestesia, 500);

        // Act
        bool resultado = _servicio.ValidadorNombre(insumoDto);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void ValidadorNombre_NombreSoloLetras_RetornaTrue()
    {
        // Arrange
        InsumoDtosInput insumoDto = new InsumoDtosInput(1, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);

        // Act
        bool resultado = _servicio.ValidadorNombre(insumoDto);

        // Assert
        Assert.True(resultado);
    }
}