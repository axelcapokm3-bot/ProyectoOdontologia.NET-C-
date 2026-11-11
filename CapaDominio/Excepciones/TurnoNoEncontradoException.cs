namespace CapaDominio.Excepciones;

public class TurnoNoEncontradoException : DomainException
{
    public int TurnoId { get; }

    public TurnoNoEncontradoException(int turnoId)
        : base($"No se encontró el turno con ID {turnoId}.")
    {
        TurnoId = turnoId;
    }
}
