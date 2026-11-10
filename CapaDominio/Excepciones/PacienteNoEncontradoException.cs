namespace CapaDominio.Excepciones;

public class PacienteNoEncontradoException : DomainException
{
    public int PacienteId { get; }

    public PacienteNoEncontradoException(int pacienteId)
        : base($"No se encontró el paciente con ID {pacienteId}.")
    {
        PacienteId = pacienteId;
    }

    public PacienteNoEncontradoException(string message)
        : base(message)
    {
        PacienteId = 0;
    }
}
