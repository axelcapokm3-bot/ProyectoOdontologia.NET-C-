namespace ProyectoOdontologia.Tests.WebApi;

using System.Threading;
using System.Threading.Tasks;
using global::CapaAplicacion.Interfaces;
using CapaWebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class TurnosControllerTests
{
    [Fact]
    public async Task Completar_TurnoValido_InvocaElServicioYResponde204()
    {
        // Arrange
        Mock<ITurnosService> servicio = new Mock<ITurnosService>();
        servicio
            .Setup(s => s.CompletarTurnoAsync(9, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        TurnosController controller = new TurnosController(servicio.Object);

        // Act
        IActionResult resultado = await controller.Completar(9, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
        servicio.Verify(
            s => s.CompletarTurnoAsync(9, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Completar_ReenviaElCancellationTokenAlServicio()
    {
        // Arrange
        Mock<ITurnosService> servicio = new Mock<ITurnosService>();
        using CancellationTokenSource origen = new CancellationTokenSource();

        TurnosController controller = new TurnosController(servicio.Object);

        // Act
        await controller.Completar(5, origen.Token);

        // Assert
        servicio.Verify(
            s => s.CompletarTurnoAsync(5, origen.Token),
            Times.Once);
    }
}
