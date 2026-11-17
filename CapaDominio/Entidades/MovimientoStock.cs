namespace CapaDominio.Entidades;

using System;

public class MovimientoStock
{
    public int Id { get; private set; }
    public int InsumoId { get; private set; }
    public TipoMovimientoStock TipoMovimiento { get; private set; }
    public int Cantidad { get; private set; }
    public int StockAnterior { get; private set; }
    public int StockNuevo { get; private set; }
    public DateTime FechaHora { get; private set; }
    public string Motivo { get; private set; }

    public MovimientoStock(
        int insumoId,
        TipoMovimientoStock tipoMovimiento,
        int cantidad,
        int stockAnterior,
        int stockNuevo,
        DateTime fechaHora,
        string motivo)
        : this(0, insumoId, tipoMovimiento, cantidad, stockAnterior, stockNuevo, fechaHora, motivo)
    {
    }

    public MovimientoStock(
        int id,
        int insumoId,
        TipoMovimientoStock tipoMovimiento,
        int cantidad,
        int stockAnterior,
        int stockNuevo,
        DateTime fechaHora,
        string motivo)
    {
        if (id < 0)
        {
            throw new ArgumentException("El ID no puede ser negativo.", nameof(id));
        }

        if (insumoId <= 0)
        {
            throw new ArgumentException("El ID del insumo debe ser mayor a cero.", nameof(insumoId));
        }

        if (!Enum.IsDefined(tipoMovimiento))
        {
            throw new ArgumentOutOfRangeException(nameof(tipoMovimiento), "El tipo de movimiento no es válido.");
        }

        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
        }

        if (stockAnterior < 0)
        {
            throw new ArgumentException("El stock anterior no puede ser negativo.", nameof(stockAnterior));
        }

        if (stockNuevo < 0)
        {
            throw new ArgumentException("El stock nuevo no puede ser negativo.", nameof(stockNuevo));
        }

        if (fechaHora == default)
        {
            throw new ArgumentException("La fecha del movimiento es obligatoria.", nameof(fechaHora));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("El motivo del movimiento es obligatorio.", nameof(motivo));
        }

        ValidarDiferencia(tipoMovimiento, cantidad, stockAnterior, stockNuevo);

        Id = id;
        InsumoId = insumoId;
        TipoMovimiento = tipoMovimiento;
        Cantidad = cantidad;
        StockAnterior = stockAnterior;
        StockNuevo = stockNuevo;
        FechaHora = fechaHora;
        Motivo = motivo;
    }

    private static void ValidarDiferencia(
        TipoMovimientoStock tipoMovimiento,
        int cantidad,
        int stockAnterior,
        int stockNuevo)
    {
        if (stockNuevo == stockAnterior)
        {
            throw new ArgumentException("El stock nuevo debe ser distinto del stock anterior.", nameof(stockNuevo));
        }

        if (tipoMovimiento == TipoMovimientoStock.Entrada && stockNuevo != stockAnterior + cantidad)
        {
            throw new ArgumentException("La entrada no coincide con la diferencia de stock.", nameof(cantidad));
        }

        if (tipoMovimiento == TipoMovimientoStock.Salida && stockNuevo != stockAnterior - cantidad)
        {
            throw new ArgumentException("La salida no coincide con la diferencia de stock.", nameof(cantidad));
        }

        if (tipoMovimiento == TipoMovimientoStock.Ajuste && cantidad != Math.Abs(stockNuevo - stockAnterior))
        {
            throw new ArgumentException("El ajuste no coincide con la diferencia de stock.", nameof(cantidad));
        }
    }
}
