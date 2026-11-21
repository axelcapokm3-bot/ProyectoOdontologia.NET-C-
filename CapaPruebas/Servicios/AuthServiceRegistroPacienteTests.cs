namespace ProyectoOdontologia.Tests.Servicios;

using global::CapaDominio.Entidades;
using global::CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using ProyectoOdontologia.Tests.Helpers;
using global::CapaAplicacion.Interfaces;
using global::CapaAplicacion.Seguridad.Interfaces;
using global::CapaAplicacion.Seguridad.Servicios;

public class AuthServiceRegistroPacienteTests
{
    private readonly Mock<global::CapaAplicacion.Interfaces.IRepositorioUsuario> _mockRepositorioUsuario;
    private readonly Mock<global::CapaAplicacion.Interfaces.IRepositorioPaciente> _mockRepositorioPaciente;
    private readonly Mock<global::CapaAplicacion.Interfaces.IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<global::CapaAplicacion.Seguridad.Interfaces.IJwtProvider> _mockJwtProvider;
    private readonly Mock<global::CapaAplicacion.Interfaces.IUnitOfWork> _mockUnitOfWork;
    private readonly AuthService _servicio;

    public AuthServiceRegistroPacienteTests()
    {
        _mockRepositorioUsuario = new Mock<IRepositorioUsuario>();
        _mockRepositorioPaciente = new Mock<IRepositorioPaciente>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtProvider = new Mock<IJwtProvider>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositorioUsuario
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);

        _mockRepositorioPaciente
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);

        _mockPasswordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hash123");

        _mockJwtProvider
            .Setup(j => j.GenerarToken(It.IsAny<Usuario>()))
            .Returns(("token", DateTime.UtcNow.AddHours(8)));

        _mockJwtProvider
            .Setup(j => j.GenerarRefreshToken())
            .Returns("refreshtoken");

        _servicio = new AuthService(
            _mockRepositorioUsuario.Object,
            _mockRepositorioPaciente.Object,
            _mockPasswordHasher.Object,
            _mockJwtProvider.Object,
            _mockUnitOfWork.Object);
    }

    private static RegistroPacienteRequestDto CrearRequest()
    {
        return new RegistroPacienteRequestDto
        {
            NombreUsuario = "paciente1",
            Email = "paciente@email.com",
            Password = "Password123",
            Nombre = "Juan",
            Apellido = "Perez",
            FechaNacimiento = new DateTime(1990, 5, 15),
            Telefono = "3515551234"
        };
    }

    [Fact]
    public async Task RegistrarPacienteAsync_DatosValidos_CreaUsuarioPacienteYPaciente()
    {
        // Arrange
        RegistroPacienteRequestDto request = CrearRequest();

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorNombreUsuarioAsync("paciente1"))
            .ReturnsAsync((Usuario?)null);

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorEmailAsync("paciente@email.com"))
            .ReturnsAsync((Usuario?)null);

        // Act
        AuthResponseDto resultado = await _servicio.RegistrarPacienteAsync(request);

        // Assert
        _mockRepositorioUsuario.Verify(
            r => r.AgregarAsync(It.Is<Usuario>(u =>
                u.Rol == RolUsuario.Paciente &&
                u.EstaActivo &&
                u.NombreUsuario == "paciente1")),
            Times.Once);

        _mockRepositorioPaciente.Verify(
            r => r.AgregarPaciente(It.Is<Paciente>(p =>
                p.Nombre == "Juan" &&
                p.Apellido == "Perez" &&
                p.UsuarioId == 1)),
            Times.Once);

        _mockUnitOfWork.Verify(u => u.IniciarTransaccionAsync(), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        Assert.Equal("token", resultado.AccessToken);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_NombreUsuarioDuplicado_LanzaDomainException()
    {
        // Arrange
        RegistroPacienteRequestDto request = CrearRequest();

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorNombreUsuarioAsync("paciente1"))
            .ReturnsAsync(new Usuario(1, "paciente1", "otro@email.com", "hash", RolUsuario.Paciente));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _servicio.RegistrarPacienteAsync(request));
        _mockRepositorioUsuario.Verify(r => r.AgregarAsync(It.IsAny<Usuario>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_EmailDuplicado_LanzaDomainException()
    {
        // Arrange
        RegistroPacienteRequestDto request = CrearRequest();

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorNombreUsuarioAsync("paciente1"))
            .ReturnsAsync((Usuario?)null);

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorEmailAsync("paciente@email.com"))
            .ReturnsAsync(new Usuario(1, "otro", "paciente@email.com", "hash", RolUsuario.Paciente));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _servicio.RegistrarPacienteAsync(request));
        _mockRepositorioUsuario.Verify(r => r.AgregarAsync(It.IsAny<Usuario>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
