namespace CapaInfraestructura.Persistencia;

using System.Globalization;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class TurnoRepositorySqlite : IRepositorioTurnos
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private readonly IUnitOfWork _unitOfWork;

    public TurnoRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Turno>> ObtenerTodos()
    {
        const string sql = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
ORDER BY FechaHoraInicio;";

        IEnumerable<Turno> turnos = await _unitOfWork.Connection.QueryAsync<Turno>(
            new CommandDefinition(sql, transaction: _unitOfWork.Transaction));

        return turnos.ToList();
    }

    public Task<Turno?> ObtenerTurnoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
WHERE Id = @Id;";

        Turno? turno = _unitOfWork.Connection.QuerySingleOrDefault<Turno>(
            sql,
            new { Id = id },
            _unitOfWork.Transaction);

        return Task.FromResult(turno);
    }

    public async Task<List<Turno>> BuscarTurnos(string criterio)
    {
        string busqueda = criterio?.Trim() ?? string.Empty;

        if (busqueda.Length == 0)
        {
            const string sqlTodos = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
ORDER BY FechaHoraInicio;";

            IEnumerable<Turno> todos = await _unitOfWork.Connection.QueryAsync<Turno>(
                new CommandDefinition(sqlTodos, transaction: _unitOfWork.Transaction));

            return todos.ToList();
        }

        int? id = int.TryParse(busqueda, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idBuscado)
            ? idBuscado
            : null;

        const string sql = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
WHERE (@Id IS NOT NULL AND Id = @Id)
   OR CAST(PacienteId AS TEXT) = @Texto
   OR CAST(OdontologoId AS TEXT) = @Texto
   OR CAST(TratamientoId AS TEXT) = @Texto
   OR strftime('%d/%m/%Y %H:%M', FechaHoraInicio) LIKE @Patron
ORDER BY FechaHoraInicio;";

        IEnumerable<Turno> turnos = await _unitOfWork.Connection.QueryAsync<Turno>(
            new CommandDefinition(
                sql,
                new { Id = id, Texto = busqueda, Patron = $"%{busqueda}%" },
                _unitOfWork.Transaction));

        return turnos.ToList();
    }

    public Task AgregarTurno(Turno turno)
    {
        return AgregarTurnoAsync(turno);
    }

    public Task ActualizarTurno(Turno turno)
    {
        return ActualizarTurnoAsync(turno);
    }

    public async Task<bool> EliminarTurno(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
DELETE FROM Turnos
WHERE Id = @Id;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _unitOfWork.Transaction));

        return filas > 0;
    }

    public int IdAutoIncrementoSincrono()
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Turnos;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    public async Task<Turno?> ObtenerTurnoPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
WHERE Id = @Id;";

        return await _unitOfWork.Connection.QuerySingleOrDefaultAsync<Turno>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Turno>> ObtenerTurnosDelDiaAsync(int odontologoId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado
FROM Turnos
WHERE OdontologoId = @OdontologoId
  AND DATE(FechaHoraInicio) = DATE(@Fecha)
ORDER BY FechaHoraInicio;";

        IEnumerable<Turno> turnos = await _unitOfWork.Connection.QueryAsync<Turno>(
            new CommandDefinition(
                sql,
                new { OdontologoId = odontologoId, Fecha = fecha.Date },
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken));

        return turnos.ToList();
    }

    public async Task<int> ObtenerProximoIdAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Turnos;";

        return await _unitOfWork.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken));
    }

    public async Task<int> AgregarTurnoAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        if (turno is null) throw new ArgumentNullException(nameof(turno));

        const string insertSql = @"
INSERT INTO Turnos (Id, FechaHoraInicio, FechaHoraFin, PacienteId, OdontologoId, TratamientoId, Estado)
VALUES (@Id, @FechaHoraInicio, @FechaHoraFin, @PacienteId, @OdontologoId, @TratamientoId, @Estado);";

        await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                insertSql,
                CrearParametros(turno),
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken));

        const string lastIdSql = @"
SELECT last_insert_rowid();";

        long idInsertado = await _unitOfWork.Connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                lastIdSql,
                transaction: _unitOfWork.Transaction,
                cancellationToken: cancellationToken));

        if (idInsertado != turno.Id)
        {
            throw new InvalidOperationException(
                $"SQLite generó el ID {idInsertado} pero el turno tenía el ID {turno.Id}.");
        }

        return (int)idInsertado;
    }

    public async Task ActualizarTurnoAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        if (turno is null) throw new ArgumentNullException(nameof(turno));

        const string sql = @"
UPDATE Turnos
SET FechaHoraInicio = @FechaHoraInicio,
    FechaHoraFin = @FechaHoraFin,
    PacienteId = @PacienteId,
    OdontologoId = @OdontologoId,
    TratamientoId = @TratamientoId,
    Estado = @Estado
WHERE Id = @Id;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                CrearParametros(turno),
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken));

        if (filas == 0)
            throw new KeyNotFoundException($"No se encontró un turno con el ID {turno.Id}.");
    }

    private static object CrearParametros(Turno turno)
    {
        return new
        {
            turno.Id,
            FechaHoraInicio = turno.FechaHoraInicio.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            FechaHoraFin = turno.FechaHoraFin.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            turno.PacienteId,
            turno.OdontologoId,
            turno.TratamientoId,
            Estado = (int)turno.Estado
        };
    }

}
