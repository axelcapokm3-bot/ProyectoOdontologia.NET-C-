namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;

public record PacienteDtoOutput(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    DateTime FechaNacimiento,
    string Telefono
);
