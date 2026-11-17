namespace ProyectoOdontologia.Tests.Persistencia;

using System.Data;
using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;
using Dapper;

public class DapperMappingSqliteTests
{
    [Fact]
    public async Task Dapper_MapeaFilasDirectamenteALasEntidadesDeDominio()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        await unitOfWork.Connection.ExecuteAsync(@"
INSERT INTO Tratamientos (Id, Descripcion, Costo, DuracionEstimadaTicks)
VALUES (1, 'Consulta', '1500.75', 27000000000);

INSERT INTO Pacientes (Id, Nombre, Apellido, FechaNacimiento, Telefono, Email)
VALUES (7, 'Paciente', 'Siete', '1990-01-01', '555-0007', 'paciente7@example.com');

INSERT INTO Odontologos (Id, Nombre, Matricula, Especialidad, Telefono)
VALUES (3, 'Odontologo Tres', 'MAT-3333', 'Odontologia General', 55500003);

INSERT INTO Turnos (Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado)
VALUES (1, '2026-06-01 09:00:00', '2026-06-01 09:45:00', 7, 3, 1, 3);");

        // Act
        Tratamiento? tratamiento = await unitOfWork.Connection.QuerySingleOrDefaultAsync<Tratamiento>(
            "SELECT Id, Descripcion, Costo, DuracionEstimadaTicks AS DuracionEstimada FROM Tratamientos WHERE Id = 1;");

        Turno? turno = await unitOfWork.Connection.QuerySingleOrDefaultAsync<Turno>(
            "SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado FROM Turnos WHERE Id = 1;");

        // Assert
        Assert.NotNull(tratamiento);
        Assert.Equal(1, tratamiento!.Id);
        Assert.Equal("Consulta", tratamiento.Descripcion);
        Assert.Equal(1500.75m, tratamiento.Costo);
        Assert.Equal(TimeSpan.FromMinutes(45), tratamiento.DuracionEstimada);

        Assert.NotNull(turno);
        Assert.Equal(1, turno!.Id);
        Assert.Equal(new DateTime(2026, 6, 1, 9, 0, 0), turno.FechaHoraInicio);
        Assert.Equal(new DateTime(2026, 6, 1, 9, 45, 0), turno.FechaHoraFin);
        Assert.Equal(7, turno.PacienteId);
        Assert.Equal(3, turno.OdontologoId);
        Assert.Equal(1, turno.TratamientoId);
        Assert.Equal(EstadoTurno.Cancelado, turno.Estado);
    }

    [Fact]
    public async Task Dapper_AlMaterializar_RespetaLasInvariantesDelDominio()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        await unitOfWork.Connection.ExecuteAsync(@"
INSERT INTO Tratamientos (Id, Descripcion, Costo, DuracionEstimadaTicks)
VALUES (1, 'Inválido', '-1', 27000000000);

INSERT INTO Pacientes (Id, Nombre, Apellido, FechaNacimiento, Telefono, Email)
VALUES (7, 'Paciente', 'Siete', '1990-01-01', '555-0007', 'paciente7@example.com');

INSERT INTO Odontologos (Id, Nombre, Matricula, Especialidad, Telefono)
VALUES (3, 'Odontologo Tres', 'MAT-3333', 'Odontologia General', 55500003);

INSERT INTO Turnos (Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado)
VALUES (1, '2026-06-01 10:00:00', '2026-06-01 09:00:00', 7, 3, 1, 1);");

        // Act / Assert
        DataException errorTratamiento = await Assert.ThrowsAsync<DataException>(() =>
            unitOfWork.Connection.QuerySingleOrDefaultAsync<Tratamiento>(
                "SELECT Id, Descripcion, Costo, DuracionEstimadaTicks AS DuracionEstimada FROM Tratamientos WHERE Id = 1;"));

        DataException errorTurno = await Assert.ThrowsAsync<DataException>(() =>
            unitOfWork.Connection.QuerySingleOrDefaultAsync<Turno>(
                "SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado FROM Turnos WHERE Id = 1;"));

        Assert.IsType<ArgumentException>(errorTratamiento.InnerException);
        Assert.IsType<ArgumentOutOfRangeException>(errorTurno.InnerException);
    }

    [Fact]
    public async Task Dapper_TratamientoInsumoInvalido_LanzaExcepcionDeInvariante()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();

        await unitOfWork.Connection.ExecuteAsync(@"
INSERT INTO Tratamientos (Id, Descripcion, Costo, DuracionEstimadaTicks)
VALUES (1, 'Consulta', '1500.75', 27000000000);

INSERT INTO Insumos (Id, Nombre, Categoria, Stock, StockReservado, StockMinimo, PuntoPedido, StockSeguridad, StockMaximo)
VALUES (1, 'Anestesia Lidocaina', 2, 100, 0, 100, 100, 50, 300);

INSERT INTO TratamientoInsumos (TratamientoId, InsumoId, CantidadUsada)
VALUES (1, 1, 0);");

        // Act
        DataException error = await Assert.ThrowsAsync<DataException>(() =>
            unitOfWork.Connection.QuerySingleOrDefaultAsync<TratamientoInsumo>(
                "SELECT TratamientoId, InsumoId, CantidadUsada FROM TratamientoInsumos WHERE TratamientoId = 1;"));

        // Assert
        Assert.IsType<ArgumentException>(error.InnerException);
    }
}
