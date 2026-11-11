namespace CapaDominio.Excepciones;

public class TratamientoNoEncontradoException : DomainException
{
    public int TratamientoId { get; }

    public TratamientoNoEncontradoException(int tratamientoId)
        : base($"No se encontró el tratamiento con ID {tratamientoId}.")
    {
        TratamientoId = tratamientoId;
    }
}
