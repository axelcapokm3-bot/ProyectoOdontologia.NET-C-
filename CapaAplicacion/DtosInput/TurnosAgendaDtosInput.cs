namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;

public record CrearTurnoRequestDto
{
    [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
    public DateTime FechaHoraInicio { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID del paciente debe ser mayor a 0.")]
    public int PacienteId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID del odontólogo debe ser mayor a 0.")]
    public int OdontologoId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID del tratamiento debe ser mayor a 0.")]
    public int TratamientoId { get; init; }
}

public record ConsultarHuecosRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ID del odontólogo debe ser mayor a 0.")]
    public int OdontologoId { get; init; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime Fecha { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La duración mínima debe ser mayor a 0 minutos.")]
    public int DuracionMinutos { get; init; }
}
