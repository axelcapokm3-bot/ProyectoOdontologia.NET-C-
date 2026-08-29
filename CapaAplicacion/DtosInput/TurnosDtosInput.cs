namespace ProyectoOdontologia.CapaAplicacion.DtosInput;

using System.ComponentModel.DataAnnotations;

public record TurnoDtoInput
{
    public TurnoDtoInput()
    {
    }

    public TurnoDtoInput(DateTime FechaHoraInicio, DateTime FechaHoraFin, int PacienteId, int OdontologoId, int TratamientoId)
    {
        this.FechaHoraInicio = FechaHoraInicio;
        this.FechaHoraFin = FechaHoraFin;
        this.PacienteId = PacienteId;
        this.OdontologoId = OdontologoId;
        this.TratamientoId = TratamientoId;
    }

    [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
    public DateTime FechaHoraInicio { get; init; } = default;

    [Required(ErrorMessage = "La fecha y hora de fin son obligatorias.")]
    public DateTime FechaHoraFin { get; init; } = default;

    [Required(ErrorMessage = "El identificador del paciente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del paciente debe ser mayor a 0.")]
    public int PacienteId { get; init; } = default;

    [Required(ErrorMessage = "El identificador del odontólogo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del odontólogo debe ser mayor a 0.")]
    public int OdontologoId { get; init; } = default;

    [Required(ErrorMessage = "El identificador del tratamiento es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del tratamiento debe ser mayor a 0.")]
    public int TratamientoId { get; init; } = default;
}
