namespace CapaDominio.Excepciones;

public class TurnoNoModificableException : DomainException
{
    public TurnoNoModificableException(string message)
        : base(message)
    {
    }
}
