namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;


// Una sola línea hace todo el trabajo sucio por ti
public record PacienteDtoOutput(int Id, string Nombre, string Apellido, string Email, DateTime FechaNacimiento, string Telefono);
