namespace CapaDominio.Excepciones;

public class OdontologoNoEncontradoException : DomainException
{
    public int OdontologoId { get; }

    public OdontologoNoEncontradoException(int odontologoId)
        : base($"No se encontró el odontólogo con ID {odontologoId}.")
    {
        OdontologoId = odontologoId;
    }
}
