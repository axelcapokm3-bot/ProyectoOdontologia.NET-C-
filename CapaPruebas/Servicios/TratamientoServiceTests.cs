namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using global::CapaAplicacion.Interfaces;

public class TratamientoServiceTests
{
    private readonly Mock<IRepositorioTratamiento> _mockTratamientos;
    private readonly Mock<IRepositorioInsumos> _mockInsumos;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TratamientoService _servicio;

    public TratamientoServiceTests()
    {
        _mockTratamientos = new Mock<IRepositorioTratamiento>();
        _mockInsumos = new Mock<IRepositorioInsumos>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockTratamientos
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorId(It.IsAny<int>()))
            .ReturnsAsync((Tratamiento?)null);
        _mockInsumos
            .Setup(r => r.ObtenerInsumoPorId(10))
            .ReturnsAsync(new Insumo(10, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500));

        _servicio = new TratamientoService(
            _mockTratamientos.Object,
            _mockInsumos.Object,
            _mockUnitOfWork.Object);
    }

    private static TratamientoDtoInput CrearDto()
    {
        return new TratamientoDtoInput
        {
            Descripcion = "Limpieza",
            Costo = 5000m,
            DuracionMinutos = 30,
            TratamientoInsumo = new List<InsumoRequeridoDtoInput>
            {
                new InsumoRequeridoDtoInput { InsumoId = 10, CantidadUsada = 2 }
            }
        };
    }

    [Fact]
    public async Task RegistrarTratamientoAsync_DatosValidos_IniciaYConfirmaTransaccion()
    {
        // Act
        await _servicio.RegistrarTratamientoAsync(CrearDto());

        // Assert
        _mockTratamientos.Verify(
            r => r.AgregarTratamiento(It.Is<Tratamiento>(t =>
                t.Descripcion == "Limpieza" &&
                t.InsumosRequeridos.Count == 1)),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarTratamientoAsync_InsumoInexistente_LanzaYNoConfirma()
    {
        // Arrange
        _mockInsumos
            .Setup(r => r.ObtenerInsumoPorId(10))
            .ReturnsAsync((Insumo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InsumoNoEncontradoException>(
            () => _servicio.RegistrarTratamientoAsync(CrearDto()));

        _mockTratamientos.Verify(r => r.AgregarTratamiento(It.IsAny<Tratamiento>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarTratamientoAsync_SinInsumos_LanzaArgumentExceptionSinAbrirTransaccion()
    {
        // Arrange
        TratamientoDtoInput dto = new TratamientoDtoInput
        {
            Descripcion = "Limpieza",
            Costo = 5000m,
            DuracionMinutos = 30,
            TratamientoInsumo = new List<InsumoRequeridoDtoInput>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.RegistrarTratamientoAsync(dto));
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarTratamientoAsync_IdInexistente_LanzaTratamientoNoEncontrado()
    {
        // Act & Assert
        await Assert.ThrowsAsync<TratamientoNoEncontradoException>(
            () => _servicio.EliminarTratamientoAsync(99));

        _mockTratamientos.Verify(r => r.EliminarTratamiento(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarTratamientoAsync_IdExistente_ConfirmaTransaccion()
    {
        // Arrange
        _mockTratamientos
            .Setup(r => r.ObtenerTratamientoPorId(1))
            .ReturnsAsync(new Tratamiento(1, "Limpieza", 5000m, new List<TratamientoInsumo>()));
        _mockTratamientos
            .Setup(r => r.EliminarTratamiento(1))
            .ReturnsAsync(true);

        // Act
        bool resultado = await _servicio.EliminarTratamientoAsync(1);

        // Assert
        Assert.True(resultado);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
