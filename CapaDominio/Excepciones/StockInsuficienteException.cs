namespace CapaDominio.Excepciones;

public class StockInsuficienteException : DomainException
{
    public int InsumoId { get; }
    public int CantidadSolicitada { get; }
    public int StockDisponible { get; }

    public StockInsuficienteException(int insumoId, int cantidadSolicitada, int stockDisponible)
        : base($"No hay stock suficiente para el insumo {insumoId}. Solicitado: {cantidadSolicitada}. Disponible: {stockDisponible}.")
    {
        InsumoId = insumoId;
        CantidadSolicitada = cantidadSolicitada;
        StockDisponible = stockDisponible;
    }
}
