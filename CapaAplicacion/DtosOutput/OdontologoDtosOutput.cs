namespace ProyectoOdontologia.CapaAplicacion.DtosOutput;


public record OdontologoOutputDto(
    Guid Id,
    string Nombre,
    string Matricula,
    string Especialidad,
    int Telefono
);