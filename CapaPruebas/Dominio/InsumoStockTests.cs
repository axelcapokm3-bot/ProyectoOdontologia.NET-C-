namespace ProyectoOdontologia.Tests.Dominio;

using CapaDominio.Entidades;
using CapaDominio.Excepciones;

public class InsumoStockTests
{
    [Fact]
    public void ConstructorCompleto_RehidrataTodosLosCampos()
    {
        // Act
        Insumo insumo = new Insumo(
            7,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            500,
            25,
            100,
            50,
            300);

        // Assert
        Assert.Equal(7, insumo.Id);
        Assert.Equal(500, insumo.Stock);
        Assert.Equal(25, insumo.StockReservado);
        Assert.Equal(100, insumo.StockMinimo);
        Assert.Equal(100, insumo.PuntoPedido);
        Assert.Equal(50, insumo.StockSeguridad);
        Assert.Equal(300, insumo.StockMaximo);
        Assert.Equal(475, insumo.StockDisponible());
    }

    [Fact]
    public void ConstructorConStockMinimo_RequiereReabastecimientoCuandoElDisponibleNoSuperaElMinimo()
    {
        // Arrange
        Insumo insumo = new Insumo(
            12,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            8,
            0,
            20,
            5,
            8,
            50);

        // Act & Assert
        Assert.Equal(8, insumo.StockMinimo);
        Assert.True(insumo.RequiereReabastecimiento());

        insumo.ReponerStock(1);

        Assert.False(insumo.RequiereReabastecimiento());
    }

    [Fact]
    public void DescontarStock_SuperaDisponible_LanzaStockInsuficienteException()
    {
        // Arrange
        Insumo insumo = new Insumo(
            9,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            50,
            45,
            100,
            50,
            300);

        // Act & Assert
        StockInsuficienteException excepcion = Assert.Throws<StockInsuficienteException>(
            () => insumo.DescontarStock(6));

        Assert.Equal(9, excepcion.InsumoId);
        Assert.Equal(6, excepcion.CantidadSolicitada);
        Assert.Equal(5, excepcion.StockDisponible);
        Assert.Equal(50, insumo.Stock);
    }

    [Fact]
    public void Actualizar_PreservaReservadoYStockMinimoYRecalculaUmbrales()
    {
        // Arrange
        Insumo insumo = new Insumo(
            3,
            "Anestesia Lidocaina",
            CategoriaInsumo.Anestesia,
            500,
            25,
            100,
            50,
            300);

        // Act
        insumo.Actualizar("Algodon", CategoriaInsumo.Descartable, 600, 30);

        // Assert
        Assert.Equal("Algodon", insumo.Nombre);
        Assert.Equal(CategoriaInsumo.Descartable, insumo.Categoria);
        Assert.Equal(600, insumo.Stock);
        Assert.Equal(30, insumo.StockReservado);
        Assert.Equal(200, insumo.StockSeguridad);
        Assert.Equal(100, insumo.StockMinimo);
        Assert.Equal(400, insumo.PuntoPedido);
        Assert.Equal(1200, insumo.StockMaximo);
    }

    [Fact]
    public void Actualizar_ConStockMinimoExplicito_LoAsigna()
    {
        // Arrange
        Insumo insumo = new Insumo(3, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);

        // Act
        insumo.Actualizar("Anestesia Lidocaina", CategoriaInsumo.Anestesia, 400, 20, 90);

        // Assert
        Assert.Equal(90, insumo.StockMinimo);
        Assert.Equal(400, insumo.Stock);
        Assert.Equal(20, insumo.StockReservado);
    }

    [Fact]
    public void Actualizar_StockMinimoMayorAlNuevoMaximo_LanzaArgumentException()
    {
        // Arrange
        Insumo insumo = new Insumo(3, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => insumo.Actualizar("Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500, 0, 400));
    }

    [Fact]
    public void Actualizar_ReservadoMayorAlStock_LanzaArgumentException()
    {
        // Arrange
        Insumo insumo = new Insumo(3, "Anestesia Lidocaina", CategoriaInsumo.Anestesia, 500);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => insumo.Actualizar("Anestesia", CategoriaInsumo.Anestesia, 10, 11));
    }
}
