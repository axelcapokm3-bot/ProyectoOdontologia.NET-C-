namespace CapaInfraestructura.Persistencia;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaInfraestructura.Servicios;
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
    Telefono TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Insumos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Categoria INTEGER NOT NULL,
    Stock INTEGER NOT NULL,
    StockReservado INTEGER NOT NULL,
    StockMinimo INTEGER NOT NULL,
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
    FOREIGN KEY (PacienteId) REFERENCES Pacientes (Id) ON DELETE RESTRICT,
    FOREIGN KEY (OdontologoId) REFERENCES Odontologos (Id) ON DELETE RESTRICT,
    FOREIGN KEY (TratamientoId) REFERENCES Tratamientos (Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreUsuario TEXT NOT NULL UNIQUE COLLATE NOCASE,
    Email TEXT NOT NULL UNIQUE COLLATE NOCASE,
    PasswordHash TEXT NOT NULL,
    Rol INTEGER NOT NULL,
    EstaActivo INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NOT NULL,
    Token TEXT NOT NULL UNIQUE,
    ExpiraUtc TEXT NOT NULL,
    Usado INTEGER NOT NULL,
    Revocado INTEGER NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id) ON DELETE CASCADE
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
    ON Insumos (Categoria);

CREATE INDEX IF NOT EXISTS IX_RefreshTokens_UsuarioId
    ON RefreshTokens (UsuarioId);";

    public static async Task InicializarAsync(
        IDbConnectionFactory connectionFactory,
        CancellationToken cancellationToken = default,
        string? passwordAdmin = null)
    {
        if (connectionFactory is null) throw new ArgumentNullException(nameof(connectionFactory));

        using IDbConnection connection = connectionFactory.CreateConnection();
        connection.Open();

        await connection.ExecuteAsync(new CommandDefinition("PRAGMA foreign_keys = OFF;"));

        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            CommandDefinition command = new CommandDefinition(
                EsquemaSql,
                transaction: transaction,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);

            await MigrarTelefonoOdontologosAsync(connection, transaction, cancellationToken);

            CommandDefinition consultaColumna = new CommandDefinition(
                "SELECT COUNT(*) FROM pragma_table_info('Insumos') WHERE name = 'StockMinimo';",
                transaction: transaction,
                cancellationToken: cancellationToken);

            long existeStockMinimo = await connection.ExecuteScalarAsync<long>(consultaColumna);

            if (existeStockMinimo == 0)
            {
                CommandDefinition agregarColumna = new CommandDefinition(
                    "ALTER TABLE Insumos ADD COLUMN StockMinimo INTEGER NOT NULL DEFAULT 0;",
                    transaction: transaction,
                    cancellationToken: cancellationToken);

                await connection.ExecuteAsync(agregarColumna);

                CommandDefinition completarValores = new CommandDefinition(
                    "UPDATE Insumos SET StockMinimo = PuntoPedido WHERE StockMinimo = 0;",
                    transaction: transaction,
                    cancellationToken: cancellationToken);

                await connection.ExecuteAsync(completarValores);
            }

            await MigrarTratamientoInsumosAsync(connection, transaction, cancellationToken);

            await AsegurarClavesForaneasTurnosAsync(connection, transaction);

            await SembrarAdminAsync(connection, transaction, passwordAdmin, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task SembrarAdminAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        string? passwordAdmin,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(passwordAdmin))
        {
            Console.WriteLine("[ADVERTENCIA] 'Seed:AdminPassword' no está configurado (User Secrets o variables de entorno). No se sembró el usuario admin.");
            return;
        }

        CommandDefinition consultaAdmin = new CommandDefinition(
            "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = 'admin';",
            transaction: transaction,
            cancellationToken: cancellationToken);

        long existeAdmin = await connection.ExecuteScalarAsync<long>(consultaAdmin);

        if (existeAdmin > 0)
            return;

        string passwordHash = new PasswordHasherBCrypt().Hash(passwordAdmin);

        CommandDefinition insertarAdmin = new CommandDefinition(
            @"
INSERT INTO Usuarios (NombreUsuario, Email, PasswordHash, Rol, EstaActivo)
VALUES ('admin', 'admin@local.com', @PasswordHash, 4, 1);",
            new { PasswordHash = passwordHash },
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(insertarAdmin);
    }

    private static async Task MigrarTelefonoOdontologosAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        CommandDefinition consultaColumnas = new CommandDefinition(
            "PRAGMA table_info('Odontologos');",
            transaction: transaction,
            cancellationToken: cancellationToken);

        IEnumerable<FilaColumnaOdontologos> columnas = await connection.QueryAsync<FilaColumnaOdontologos>(consultaColumnas);

        bool tablaExiste = false;
        bool telefonoEsTexto = false;

        foreach (FilaColumnaOdontologos columna in columnas)
        {
            tablaExiste = true;

            if (string.Equals(columna.Name, "Telefono", StringComparison.OrdinalIgnoreCase))
            {
                string tipoColumna = columna.Type ?? string.Empty;

                if (tipoColumna.ToUpperInvariant().Contains("TEXT"))
                {
                    telefonoEsTexto = true;
                }
            }
        }

        if (!tablaExiste || telefonoEsTexto)
            return;

        const string migracionSql = @"
DROP TABLE IF EXISTS Odontologos_Nuevo;

CREATE TABLE Odontologos_Nuevo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NULL,
    Nombre TEXT NOT NULL,
    Matricula TEXT NOT NULL,
    Especialidad TEXT NOT NULL,
    Telefono TEXT NOT NULL
);

INSERT INTO Odontologos_Nuevo (Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono)
SELECT Id, UsuarioId, Nombre, Matricula, Especialidad, CAST(Telefono AS TEXT)
FROM Odontologos;

DROP TABLE Odontologos;

ALTER TABLE Odontologos_Nuevo RENAME TO Odontologos;

CREATE INDEX IF NOT EXISTS IX_Odontologos_Matricula
    ON Odontologos (Matricula);";

        CommandDefinition migracion = new CommandDefinition(
            migracionSql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(migracion);
    }

    private static async Task MigrarTratamientoInsumosAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        CommandDefinition consultaRestriccion = new CommandDefinition(
            "SELECT COUNT(*) FROM pragma_foreign_key_list('TratamientoInsumos') WHERE \"table\" = 'Insumos' AND on_delete = 'RESTRICT';",
            transaction: transaction,
            cancellationToken: cancellationToken);

        long existeRestriccion = await connection.ExecuteScalarAsync<long>(consultaRestriccion);

        if (existeRestriccion > 0)
            return;

        const string migracionSql = @"
DROP TABLE IF EXISTS TratamientoInsumos_Nuevo;

CREATE TABLE TratamientoInsumos_Nuevo (
    TratamientoId INTEGER NOT NULL,
    InsumoId INTEGER NOT NULL,
    CantidadUsada INTEGER NOT NULL,
    PRIMARY KEY (TratamientoId, InsumoId),
    FOREIGN KEY (TratamientoId) REFERENCES Tratamientos (Id) ON DELETE CASCADE,
    FOREIGN KEY (InsumoId) REFERENCES Insumos (Id) ON DELETE RESTRICT
);

INSERT INTO TratamientoInsumos_Nuevo (TratamientoId, InsumoId, CantidadUsada)
SELECT ti.TratamientoId, ti.InsumoId, ti.CantidadUsada
FROM TratamientoInsumos ti
WHERE EXISTS (SELECT 1 FROM Tratamientos t WHERE t.Id = ti.TratamientoId)
  AND EXISTS (SELECT 1 FROM Insumos i WHERE i.Id = ti.InsumoId);

DROP TABLE TratamientoInsumos;

ALTER TABLE TratamientoInsumos_Nuevo RENAME TO TratamientoInsumos;

CREATE INDEX IF NOT EXISTS IX_TratamientoInsumos_InsumoId
    ON TratamientoInsumos (InsumoId);";

        CommandDefinition migracion = new CommandDefinition(
            migracionSql,
            transaction: transaction,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(migracion);
    }

    private static async Task AsegurarClavesForaneasTurnosAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        CommandDefinition consultaClaves = new CommandDefinition(
            "PRAGMA foreign_key_list('Turnos');",
            transaction: transaction);

        IEnumerable<FilaClaveForanea> filas = await connection.QueryAsync<FilaClaveForanea>(consultaClaves);

        bool referenciaPacientes = false;
        bool referenciaOdontologos = false;
        bool referenciaTratamientos = false;

        foreach (FilaClaveForanea fila in filas)
        {
            if (string.Equals(fila.Table, "Pacientes", StringComparison.OrdinalIgnoreCase))
            {
                referenciaPacientes = true;
            }
            else if (string.Equals(fila.Table, "Odontologos", StringComparison.OrdinalIgnoreCase))
            {
                referenciaOdontologos = true;
            }
            else if (string.Equals(fila.Table, "Tratamientos", StringComparison.OrdinalIgnoreCase))
            {
                referenciaTratamientos = true;
            }
        }

        if (referenciaPacientes && referenciaOdontologos && referenciaTratamientos)
            return;

        const string migracionSql = @"
DROP TABLE IF EXISTS Turnos_Nuevo;

CREATE TABLE Turnos_Nuevo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FechaHoraInicio TEXT NOT NULL,
    FechaHoraFin TEXT NOT NULL,
    PacienteId INTEGER NOT NULL,
    OdontologoId INTEGER NOT NULL,
    TratamientoId INTEGER NOT NULL,
    Estado INTEGER NOT NULL,
    FOREIGN KEY (PacienteId) REFERENCES Pacientes (Id) ON DELETE RESTRICT,
    FOREIGN KEY (OdontologoId) REFERENCES Odontologos (Id) ON DELETE RESTRICT,
    FOREIGN KEY (TratamientoId) REFERENCES Tratamientos (Id) ON DELETE RESTRICT
);

INSERT INTO Turnos_Nuevo (Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado)
SELECT t.Id, t.FechaHoraInicio, t.FechaHoraFin, t.PacienteId, t.OdontologoId, t.TratamientoId, t.Estado
FROM Turnos t
INNER JOIN Pacientes p ON p.Id = t.PacienteId
INNER JOIN Odontologos o ON o.Id = t.OdontologoId
INNER JOIN Tratamientos tr ON tr.Id = t.TratamientoId;

DROP TABLE Turnos;

ALTER TABLE Turnos_Nuevo RENAME TO Turnos;

CREATE INDEX IF NOT EXISTS IX_Turnos_OdontologoId_FechaHoraInicio
    ON Turnos (OdontologoId, FechaHoraInicio);

CREATE INDEX IF NOT EXISTS IX_Turnos_FechaHoraInicio
    ON Turnos (FechaHoraInicio);

CREATE INDEX IF NOT EXISTS IX_Turnos_PacienteId
    ON Turnos (PacienteId);

CREATE INDEX IF NOT EXISTS IX_Turnos_TratamientoId
    ON Turnos (TratamientoId);";

        CommandDefinition migracion = new CommandDefinition(
            migracionSql,
            transaction: transaction);

        await connection.ExecuteAsync(migracion);
    }

    private sealed class FilaColumnaOdontologos
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }

    private sealed class FilaClaveForanea
    {
        public string Table { get; set; } = string.Empty;
    }
}
