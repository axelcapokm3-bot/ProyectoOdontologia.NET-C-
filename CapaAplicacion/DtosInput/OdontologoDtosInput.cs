namespace ProyectoOdontologia.CapaAplicacion.DtosInput;



public record OdontologoInputDto(
    string Nombre,
    string Matricula,
    string Especialidad,
    int Telefono
);