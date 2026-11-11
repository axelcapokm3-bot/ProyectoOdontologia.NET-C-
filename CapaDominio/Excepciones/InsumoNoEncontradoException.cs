namespace CapaDominio.Excepciones;

public class InsumoNoEncontradoException : DomainException
{
    public int InsumoId { get; }

    public InsumoNoEncontradoException(int insumoId)
        : base($"No se encontró el insumo con ID {insumoId}.")
    {
        InsumoId = insumoId;
    }
}
