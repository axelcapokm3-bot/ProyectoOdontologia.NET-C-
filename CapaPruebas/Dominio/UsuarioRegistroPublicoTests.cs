namespace ProyectoOdontologia.Tests.Dominio;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;

public class UsuarioRegistroPublicoTests
{
    [Fact]
    public void CrearParaRegistroPublico_RolEsPacienteYActivo()
    {
        // Arrange & Act
        Usuario usuario = Usuario.CrearParaRegistroPublico(1, "paciente1", "paciente@email.com", "hash123");

        // Assert
        Assert.Equal(1, usuario.Id);
        Assert.Equal("paciente1", usuario.NombreUsuario);
        Assert.Equal("paciente@email.com", usuario.Email);
        Assert.Equal(RolUsuario.Paciente, usuario.Rol);
        Assert.True(usuario.EstaActivo);
    }

    [Fact]
    public void AsignarRol_MismoRol_LanzaDomainException()
    {
        // Arrange
        Usuario usuario = Usuario.CrearParaRegistroPublico(1, "paciente1", "paciente@email.com", "hash123");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => usuario.AsignarRol(RolUsuario.Paciente));
        Assert.Equal("El usuario ya tiene asignado ese rol.", ex.Message);
    }

    [Fact]
    public void AsignarRol_UsuarioPacienteACambiarRol_ActualizaCorrectamente()
    {
        // Arrange
        Usuario usuario = Usuario.CrearParaRegistroPublico(1, "paciente1", "paciente@email.com", "hash123");

        // Act
        usuario.AsignarRol(RolUsuario.Odontologo);

        // Assert
        Assert.Equal(RolUsuario.Odontologo, usuario.Rol);
    }

    [Fact]
    public void AsignarRol_UsuarioAdminNoPuedeSerDegradado_LanzaDomainException()
    {
        // Arrange
        Usuario admin = new Usuario(1, "admin", "admin@email.com", "hash", RolUsuario.Admin);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => admin.AsignarRol(RolUsuario.Paciente));
        Assert.Equal("No se puede modificar el rol del administrador principal del sistema.", ex.Message);
        Assert.Equal(RolUsuario.Admin, admin.Rol);
    }

    [Fact]
    public void AsignarRol_RolInvalido_LanzaArgumentException()
    {
        // Arrange
        Usuario usuario = Usuario.CrearParaRegistroPublico(1, "paciente1", "paciente@email.com", "hash123");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => usuario.AsignarRol((RolUsuario)99));
    }
}
