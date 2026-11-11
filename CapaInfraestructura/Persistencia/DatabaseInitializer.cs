namespace CapaInfraestructura.Persistencia;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using Dapper;

public static class DatabaseInitializer
{
    private const string EsquemaSql = @"
CREATE TABLE IF NOT EXISTS Pacientes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NULL,
    Nombre TEXT NOT NULL,
    Apellido TEXT NOT NULL,
    FechaNacimiento TEXT NOT NULL,
    Telefono TEXT NOT NULL,
    Email TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Odontologos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NULL,
    Nombre TEXT NOT NULL,
    Matricula TEXT NOT NULL,
    Especialidad TEXT NOT NULL,
    Telefono INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Insumos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Categoria INTEGER NOT NULL,
    Stock INTEGER NOT NULL,
    StockReservado INTEGER NOT NULL,
    PuntoPedido INTEGER NOT NULL,
    StockSeguridad INTEGER NOT NULL,
    StockMaximo INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Tratamientos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Descripcion TEXT NOT NULL,
    Costo TEXT NOT NULL,
    DuracionEstimadaTicks INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS TratamientoInsumos (
    TratamientoId INTEGER NOT NULL,
    InsumoId INTEGER NOT NULL,
    CantidadUsada INTEGER NOT NULL,
    PRIMARY KEY (TratamientoId, InsumoId),
    FOREIGN KEY (TratamientoId) REFERENCES Tratamientos (Id) ON DELETE CASCADE,
    FOREIGN KEY (InsumoId) REFERENCES Insumos (Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS MovimientosStock (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InsumoId INTEGER NOT NULL,
    TipoMovimiento INTEGER NOT NULL,
    Cantidad INTEGER NOT NULL,
    StockAnterior INTEGER NOT NULL,
    StockNuevo INTEGER NOT NULL,
    FechaHora TEXT NOT NULL,
    Motivo TEXT NOT NULL,
    FOREIGN KEY (InsumoId) REFERENCES Insumos (Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Turnos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FechaHoraInicio TEXT NOT NULL,
    FechaHoraFin TEXT NOT NULL,
    PacienteId INTEGER NOT NULL,
    OdontologoId INTEGER NOT NULL,
    TratamientoId INTEGER NOT NULL,
    Estado INTEGER NOT NULL,
    FOREIGN KEY (TratamientoId) REFERENCES Tratamientos (Id)
);

CREATE INDEX IF NOT EXISTS IX_Turnos_OdontologoId_FechaHoraInicio
    ON Turnos (OdontologoId, FechaHoraInicio);

CREATE INDEX IF NOT EXISTS IX_Turnos_FechaHoraInicio
    ON Turnos (FechaHoraInicio);

CREATE INDEX IF NOT EXISTS IX_Turnos_PacienteId
    ON Turnos (PacienteId);

CREATE INDEX IF NOT EXISTS IX_Turnos_TratamientoId
    ON Turnos (TratamientoId);

CREATE INDEX IF NOT EXISTS IX_TratamientoInsumos_InsumoId
    ON TratamientoInsumos (InsumoId);

CREATE INDEX IF NOT EXISTS IX_MovimientosStock_InsumoId_FechaHora
    ON MovimientosStock (InsumoId, FechaHora);

CREATE INDEX IF NOT EXISTS IX_Tratamientos_Descripcion
    ON Tratamientos (Descripcion);

CREATE INDEX IF NOT EXISTS IX_Pacientes_Apellido_Nombre
    ON Pacientes (Apellido, Nombre);

CREATE INDEX IF NOT EXISTS IX_Odontologos_Matricula
    ON Odontologos (Matricula);

CREATE INDEX IF NOT EXISTS IX_Insumos_Categoria
    ON Insumos (Categoria);";

    public static async Task InicializarAsync(IDbConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
    {
        if (connectionFactory is null) throw new ArgumentNullException(nameof(connectionFactory));

        using IDbConnection connection = connectionFactory.CreateConnection();
        connection.Open();

        CommandDefinition command = new CommandDefinition(
            EsquemaSql,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
