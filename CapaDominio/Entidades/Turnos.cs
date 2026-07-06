
namespace CapaDominio.Entidades;

public class Turnos
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public int PacienteId { get; set; }
    public int OdontologoId { get; set; }
    public int TratamientoId { get; set; }

    public Turnos(int id, DateTime fechaHora, int pacienteId, int odontologoId, int tratamientoId)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser un número positivo");
        }

        if (fechaHora < DateTime.Now)
        {
            throw new ArgumentException("La fecha y hora del turno no puede ser en el pasado");
        }

        if (pacienteId <= 0)
        {
            throw new ArgumentException("El ID del paciente debe ser un número positivo");
        }

        if (odontologoId <= 0)
        {
            throw new ArgumentException("El ID del odontólogo debe ser un número positivo");
        }

        if (tratamientoId <= 0)
        {
            throw new ArgumentException("El ID del tratamiento debe ser un número positivo");
        }

        Id = id;
        FechaHora = fechaHora;
        PacienteId = pacienteId;
        OdontologoId = odontologoId;
        TratamientoId = tratamientoId;
    }


}