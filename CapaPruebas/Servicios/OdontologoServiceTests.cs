namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using global::CapaAplicacion.Interfaces;

public class OdontologoServiceTests
{
    private readonly Mock<IRepositorioOdontologo> _mockOdontologos;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly OdontologoService _servicio;

    public OdontologoServiceTests()
    {
        _mockOdontologos = new Mock<IRepositorioOdontologo>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockOdontologos
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Odontologo>());
        _mockOdontologos
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _mockOdontologos
            .Setup(r => r.ObtenerOdontologoPorId(It.IsAny<int>()))
            .ReturnsAsync((Odontologo?)null);

        _servicio = new OdontologoService(
            _mockOdontologos.Object,
            Mock.Of<IRepositorioUsuario>(),
            Mock.Of<IPasswordHasher>(),
            _mockUnitOfWork.Object);
    }

    private static OdontologoInputDto CrearDto()
    {
        return new OdontologoInputDto(0, "Ana Gomez", "MP-1234", "Ortodoncia", "351555123");
    }

    [Fact]
    public async Task RegistrarOdontologoAsync_DatosValidos_IniciaYConfirmaTransaccion()
    {
        // Act
        await _servicio.RegistrarOdontologoAsync(CrearDto());

        // Assert
        _mockOdontologos.Verify(
            r => r.AgregarOdontologo(It.Is<Odontologo>(o =>
                o.Id == 1 &&
                o.Matricula == "MP-1234")),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerOdontologoPorIdAsync_IdInexistente_LanzaOdontologoNoEncontrado()
    {
        // Act & Assert
        await Assert.ThrowsAsync<OdontologoNoEncontradoException>(
            () => _servicio.ObtenerOdontologoPorIdAsync(99));
    }

    [Fact]
    public async Task EliminarOdontologoAsync_IdInexistente_LanzaOdontologoNoEncontrado()
    {
        // Act & Assert
        await Assert.ThrowsAsync<OdontologoNoEncontradoException>(
            () => _servicio.EliminarOdontologoAsync(99));

        _mockOdontologos.Verify(r => r.EliminarOdontologo(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarOdontologoAsync_IdExistente_ConfirmaTransaccion()
    {
        // Arrange
        _mockOdontologos
            .Setup(r => r.ObtenerOdontologoPorId(1))
            .ReturnsAsync(new Odontologo(1, "Ana Gomez", "MP-1234", "Ortodoncia", "351555123"));

        // Act
        await _servicio.ActualizarOdontologoAsync(
            1,
            new OdontologoInputDto(1, "Ana Perez", "MP-1234", "Ortodoncia", "351555123"));

        // Assert
        _mockOdontologos.Verify(
            r => r.ActualizarOdontologo(It.Is<Odontologo>(o => o.Nombre == "Ana Perez")),
            Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
