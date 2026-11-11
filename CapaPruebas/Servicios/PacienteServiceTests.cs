namespace ProyectoOdontologia.Tests.Servicios;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using Moq;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using ProyectoOdontologia.Tests.Helpers;
using global::CapaAplicacion.Interfaces;

/// <summary>
/// Pruebas de <see cref="PacienteService"/> con Moq sobre <see cref="IRepositorioPaciente"/>.
/// Cubre la correccion de validacion incondicional: <c>ValidadorDeEmail</c> y
/// <c>ValidarTelefono</c> deben ejecutarse SIEMPRE antes de bifurcar el flujo de
/// creacion o actualizacion, sin importar si el ID enviado existe o no.
/// Patron AAA.
/// </summary>
public class PacienteServiceTests
{
    private readonly Mock<IRepositorioPaciente> _mockRepositorio;
    private readonly PacienteService _servicio;

    public PacienteServiceTests()
    {
        _mockRepositorio = new Mock<IRepositorioPaciente>();
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente>());
        _mockRepositorio
            .Setup(r => r.IdAutoIncrementoSincrono())
            .Returns(1);
        _servicio = new PacienteService(
            _mockRepositorio.Object,
            Mock.Of<IRepositorioUsuario>(),
            Mock.Of<IPasswordHasher>(),
            new UnitOfWorkNulo());
    }

    private static PacienteDtoInput CrearPacienteDto(string email = "juan.perez@email.com", string telefono = "3515551234")
    {
        return new PacienteDtoInput(1, "Juan", "Perez", email, new DateTime(1990, 5, 15), telefono);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_PacienteValido_InvocaAgregarPaciente()
    {
        // Arrange
        PacienteDtoInput dto = CrearPacienteDto();

        // Act
        await _servicio.RegistrarPacienteAsync(dto);

        // Assert
        _mockRepositorio.Verify(
            r => r.AgregarPaciente(It.Is<Paciente>(p => p.Id == 1 && p.Email == "juan.perez@email.com")),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarPacienteAsync_EmailYaRegistradoPorOtroPaciente_LanzaInvalidOperationException()
    {
        // Arrange: otro paciente (id 2) ya posee el email del DTO.
        Paciente existente = EntidadesFactory.CrearPaciente(id: 2);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente> { existente });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.RegistrarPacienteAsync(CrearPacienteDto()));
        _mockRepositorio.Verify(r => r.AgregarPaciente(It.IsAny<Paciente>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPacienteAsync_EmailDuplicado_LanzaInvalidOperationExceptionYNoActualiza()
    {
        // Arrange: otro paciente (id 2) tiene el mismo email que el DTO.
        Paciente otroPaciente = EntidadesFactory.CrearPaciente(id: 2);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente> { otroPaciente });
        _mockRepositorio
            .Setup(r => r.ObtenerPacientePorId(1))
            .ReturnsAsync(EntidadesFactory.CrearPaciente(id: 1));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.ActualizarPacienteAsync(1, CrearPacienteDto()));
        _mockRepositorio.Verify(r => r.ActualizarPaciente(It.IsAny<Paciente>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPacienteAsync_TelefonoDuplicado_LanzaInvalidOperationExceptionYNoActualiza()
    {
        // Arrange: email distinto, pero el telefono ya pertenece a otro paciente.
        Paciente otroPaciente = EntidadesFactory.CrearPaciente(id: 2);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente> { otroPaciente });
        _mockRepositorio
            .Setup(r => r.ObtenerPacientePorId(1))
            .ReturnsAsync(EntidadesFactory.CrearPaciente(id: 1));
        PacienteDtoInput dto = CrearPacienteDto(email: "otro@email.com");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.ActualizarPacienteAsync(1, dto));
        _mockRepositorio.Verify(r => r.ActualizarPaciente(It.IsAny<Paciente>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPacienteAsync_ValidacionDeUnicidad_SeEjecutaAunqueElIdNoExista()
    {
        // Arrange: el ID 99 no existe, pero hay otro paciente con el mismo email.
        // La validacion debe ejecutarse ANTES del check de existencia.
        Paciente otroPaciente = EntidadesFactory.CrearPaciente(id: 2);
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente> { otroPaciente });
        _mockRepositorio
            .Setup(r => r.ObtenerPacientePorId(99))
            .ReturnsAsync((Paciente?)null);

        // Act & Assert: gana la validacion de unicidad, no el KeyNotFoundException.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _servicio.ActualizarPacienteAsync(99, CrearPacienteDto()));
        _mockRepositorio.Verify(r => r.ActualizarPaciente(It.IsAny<Paciente>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPacienteAsync_DatosValidosIdExistente_ActualizaElPaciente()
    {
        // Arrange: el repositorio solo contiene al propio paciente (mismos email/telefono).
        _mockRepositorio
            .Setup(r => r.ObtenerTodos())
            .ReturnsAsync(new List<Paciente> { EntidadesFactory.CrearPaciente(id: 1) });
        _mockRepositorio
            .Setup(r => r.ObtenerPacientePorId(1))
            .ReturnsAsync(EntidadesFactory.CrearPaciente(id: 1));

        // Act
        await _servicio.ActualizarPacienteAsync(1, CrearPacienteDto());

        // Assert
        _mockRepositorio.Verify(
            r => r.ActualizarPaciente(It.Is<Paciente>(p => p.Id == 1 && p.Nombre == "Juan")),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarPacienteAsync_IdInexistenteSinDuplicados_LanzaPacienteNoEncontradoException()
    {
        // Arrange
        _mockRepositorio
            .Setup(r => r.ObtenerPacientePorId(99))
            .ReturnsAsync((Paciente?)null);

        // Act & Assert
        await Assert.ThrowsAsync<PacienteNoEncontradoException>(() => _servicio.ActualizarPacienteAsync(99, CrearPacienteDto()));
        _mockRepositorio.Verify(r => r.ActualizarPaciente(It.IsAny<Paciente>()), Times.Never);
    }
}