namespace ProyectoOdontologia.Tests.Servicios;

using global::CapaDominio.Entidades;
using global::CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaAplicacion.Interfaces;
using global::CapaAplicacion.Seguridad.Servicios;

public class UsuarioAdminServiceCambiarRolTests
{
    private readonly Mock<global::CapaAplicacion.Interfaces.IRepositorioUsuario> _mockRepositorioUsuario;
    private readonly UsuarioAdminService _servicio;

    public UsuarioAdminServiceCambiarRolTests()
    {
        _mockRepositorioUsuario = new Mock<global::CapaAplicacion.Interfaces.IRepositorioUsuario>();
        _servicio = new UsuarioAdminService(_mockRepositorioUsuario.Object);
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_UsuarioExistente_ActualizaRol()
    {
        // Arrange
        Usuario usuario = new Usuario(1, "juan", "juan@email.com", "hash", RolUsuario.Paciente);

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(usuario);

        // Act
        await _servicio.CambiarRolUsuarioAsync(1, RolUsuario.Odontologo);

        // Assert
        Assert.Equal(RolUsuario.Odontologo, usuario.Rol);
        _mockRepositorioUsuario.Verify(r => r.ActualizarAsync(usuario), Times.Once);
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_UsuarioInexistente_LanzaKeyNotFoundException()
    {
        // Arrange
        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorIdAsync(99))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _servicio.CambiarRolUsuarioAsync(99, RolUsuario.Odontologo));
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_RolIgual_LanzaDomainException()
    {
        // Arrange
        Usuario usuario = new Usuario(1, "juan", "juan@email.com", "hash", RolUsuario.Paciente);

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(usuario);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _servicio.CambiarRolUsuarioAsync(1, RolUsuario.Paciente));
    }

    [Fact]
    public async Task CambiarRolUsuarioAsync_AdministradorNoPuedeSerDegradado_LanzaDomainException()
    {
        // Arrange
        Usuario admin = new Usuario(1, "admin", "admin@email.com", "hash", RolUsuario.Admin);

        _mockRepositorioUsuario
            .Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(admin);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _servicio.CambiarRolUsuarioAsync(1, RolUsuario.Paciente));
    }
}
