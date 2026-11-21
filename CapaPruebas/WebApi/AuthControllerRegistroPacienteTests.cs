namespace ProyectoOdontologia.Tests.WebApi;

using System.Threading.Tasks;
using CapaWebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaAplicacion.Seguridad.Interfaces;

public class AuthControllerRegistroPacienteTests
{
    [Fact]
    public async Task RegistroPaciente_ModeloValido_DevuelveCreated()
    {
        // Arrange
        Mock<IAuthService> authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.RegistrarPacienteAsync(It.IsAny<RegistroPacienteRequestDto>()))
            .ReturnsAsync(new AuthResponseDto
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                UsuarioId = 1,
                NombreUsuario = "paciente1",
                Rol = "Paciente"
            });

        AuthController controller = new AuthController(authService.Object);

        RegistroPacienteRequestDto request = new()
        {
            NombreUsuario = "paciente1",
            Email = "paciente@email.com",
            Password = "Password123",
            Nombre = "Juan",
            Apellido = "Perez",
            FechaNacimiento = new System.DateTime(1990, 5, 15),
            Telefono = "3515551234"
        };

        // Act
        IActionResult resultado = await controller.RegistroPaciente(request);

        // Assert
        CreatedResult created = Assert.IsType<CreatedResult>(resultado);
        authService.Verify(
            s => s.RegistrarPacienteAsync(It.Is<RegistroPacienteRequestDto>(r =>
                r.NombreUsuario == "paciente1" &&
                r.Email == "paciente@email.com")),
            Times.Once);
    }
}

public class UsuariosControllerCambiarRolTests
{
    [Fact]
    public async Task CambiarRol_ModeloValido_DevuelveNoContent()
    {
        // Arrange
        Mock<IUsuarioAdminService> usuarioService = new Mock<IUsuarioAdminService>();
        usuarioService
            .Setup(s => s.CambiarRolUsuarioAsync(1, It.IsAny<CapaDominio.Entidades.RolUsuario>()))
            .Returns(Task.CompletedTask);

        UsuariosController controller = new UsuariosController(usuarioService.Object);

        CambiarRolRequestDto dto = new() { Rol = CapaDominio.Entidades.RolUsuario.Odontologo };

        // Act
        IActionResult resultado = await controller.CambiarRol(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
        usuarioService.Verify(
            s => s.CambiarRolUsuarioAsync(1, CapaDominio.Entidades.RolUsuario.Odontologo),
            Times.Once);
    }
}
