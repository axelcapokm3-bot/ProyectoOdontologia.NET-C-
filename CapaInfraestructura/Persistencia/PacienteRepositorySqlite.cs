namespace CapaInfraestructura.Persistencia;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class PacienteRepositorySqlite : IRepositorioPaciente
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private readonly IUnitOfWork _unitOfWork;

    public PacienteRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Paciente>> ObtenerTodos()
    {
        const string sql = @"
SELECT Id, UsuarioId, Nombre, Apellido, FechaNacimiento, Telefono, Email
FROM Pacientes
ORDER BY Apellido, Nombre;";

        IEnumerable<PacienteRow> resultado = await _unitOfWork.Connection.QueryAsync<PacienteRow>(
            new CommandDefinition(sql, transaction: _unitOfWork.Transaction));

        List<Paciente> pacientes = new List<Paciente>();

        foreach (PacienteRow fila in resultado)
        {
            pacientes.Add(Convertir(fila));
        }

        return pacientes;
    }

    public async Task<Paciente?> ObtenerPacientePorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
SELECT Id, UsuarioId, Nombre, Apellido, FechaNacimiento, Telefono, Email
FROM Pacientes
WHERE Id = @Id;";

        PacienteRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<PacienteRow>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        if (fila == null)
            return null;

        return Convertir(fila);
    }

    public async Task<List<Paciente>> BuscarPacientes(string criterio)
    {
        string busqueda = criterio?.Trim() ?? string.Empty;

        if (busqueda.Length == 0)
        {
            IEnumerable<Paciente> todos = await ObtenerTodos();
            return new List<Paciente>(todos);
        }

        const string sql = @"
SELECT Id, UsuarioId, Nombre, Apellido, FechaNacimiento, Telefono, Email
FROM Pacientes
WHERE CAST(Id AS TEXT) = @Texto
   OR Nombre LIKE @Patron
   OR Apellido LIKE @Patron
   OR Email LIKE @Patron
   OR Telefono LIKE @Patron
ORDER BY Apellido, Nombre;";

        IEnumerable<PacienteRow> resultado = await _unitOfWork.Connection.QueryAsync<PacienteRow>(
            new CommandDefinition(
                sql,
                new
                {
                    Texto = busqueda,
                    Patron = "%" + busqueda + "%"
                },
                _unitOfWork.Transaction));

        List<Paciente> pacientes = new List<Paciente>();

        foreach (PacienteRow fila in resultado)
        {
            pacientes.Add(Convertir(fila));
        }

        return pacientes;
    }

    public async Task AgregarPaciente(Paciente paciente)
    {
        if (paciente == null)
            throw new ArgumentNullException(nameof(paciente));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
INSERT INTO Pacientes (Id, UsuarioId, Nombre, Apellido, FechaNacimiento, Telefono, Email)
VALUES (@Id, @UsuarioId, @Nombre, @Apellido, @FechaNacimiento, @Telefono, @Email);";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(paciente),
                    _unitOfWork.Transaction));

            if (filas != 1)
                throw new InvalidOperationException($"No se pudo insertar el paciente con ID {paciente.Id}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task ActualizarPaciente(Paciente paciente)
    {
        if (paciente == null)
            throw new ArgumentNullException(nameof(paciente));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
UPDATE Pacientes
SET UsuarioId = @UsuarioId,
    Nombre = @Nombre,
    Apellido = @Apellido,
    FechaNacimiento = @FechaNacimiento,
    Telefono = @Telefono,
    Email = @Email
WHERE Id = @Id;";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(paciente),
                    _unitOfWork.Transaction));

            if (filas == 0)
                throw new KeyNotFoundException($"No se encontró un paciente con el ID {paciente.Id}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task<bool> EliminarPaciente(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
DELETE FROM Pacientes
WHERE Id = @Id;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        return filas > 0;
    }

    public int IdAutoIncrementoSincrono()
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Pacientes;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    public async Task<Paciente?> ObtenerPacientePorUsuarioIdAsync(int usuarioId)
    {
        const string sql = @"
SELECT Id, UsuarioId, Nombre, Apellido, FechaNacimiento, Telefono, Email
FROM Pacientes
WHERE UsuarioId = @UsuarioId
LIMIT 1;";

        PacienteRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<PacienteRow>(
            new CommandDefinition(
                sql,
                new { UsuarioId = usuarioId },
                _unitOfWork.Transaction));

        if (fila == null)
            return null;

        return Convertir(fila);
    }

    private static Paciente Convertir(PacienteRow fila)
    {
        int id = checked((int)fila.Id);
        int? usuarioId = null;

        if (fila.UsuarioId.HasValue)
            usuarioId = checked((int)fila.UsuarioId.Value);

        DateTime fechaNacimiento = DateTime.ParseExact(
            fila.FechaNacimiento,
            FormatoFecha,
            CultureInfo.InvariantCulture);

        return new Paciente(
            id,
            fila.Nombre,
            fila.Apellido,
            fechaNacimiento,
            fila.Telefono,
            fila.Email,
            usuarioId);
    }

    private static object CrearParametros(Paciente paciente)
    {
        return new
        {
            Id = paciente.Id,
            UsuarioId = paciente.UsuarioId,
            Nombre = paciente.Nombre,
            Apellido = paciente.Apellido,
            FechaNacimiento = paciente.FechaNacimiento.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            Telefono = paciente.Telefono,
            Email = paciente.Email
        };
    }
}

internal sealed class PacienteRow
{
    public long Id { get; set; }
    public long? UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string FechaNacimiento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
