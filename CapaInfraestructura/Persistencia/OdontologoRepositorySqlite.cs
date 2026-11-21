namespace CapaInfraestructura.Persistencia;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class OdontologoRepositorySqlite : IRepositorioOdontologo
{
    private readonly IUnitOfWork _unitOfWork;

    public OdontologoRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Odontologo>> ObtenerTodos()
    {
        const string sql = @"
SELECT Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono
FROM Odontologos
ORDER BY Nombre;";

        IEnumerable<OdontologoRow> resultado = await _unitOfWork.Connection.QueryAsync<OdontologoRow>(
            new CommandDefinition(sql, transaction: _unitOfWork.Transaction));

        List<Odontologo> odontologos = new List<Odontologo>();

        foreach (OdontologoRow fila in resultado)
        {
            odontologos.Add(Convertir(fila));
        }

        return odontologos;
    }

    public async Task<Odontologo?> ObtenerOdontologoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
SELECT Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono
FROM Odontologos
WHERE Id = @Id;";

        OdontologoRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<OdontologoRow>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        if (fila == null)
            return null;

        return Convertir(fila);
    }

    public async Task<List<Odontologo>> BuscarOdontologos(string criterio)
    {
        string busqueda = criterio?.Trim() ?? string.Empty;

        if (busqueda.Length == 0)
        {
            IEnumerable<Odontologo> todos = await ObtenerTodos();
            return new List<Odontologo>(todos);
        }

        const string sql = @"
SELECT Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono
FROM Odontologos
WHERE CAST(Id AS TEXT) = @Texto
   OR Nombre LIKE @Patron
   OR Matricula LIKE @Patron
   OR Especialidad LIKE @Patron
ORDER BY Nombre;";

        IEnumerable<OdontologoRow> resultado = await _unitOfWork.Connection.QueryAsync<OdontologoRow>(
            new CommandDefinition(
                sql,
                new
                {
                    Texto = busqueda,
                    Patron = "%" + busqueda + "%"
                },
                _unitOfWork.Transaction));

        List<Odontologo> odontologos = new List<Odontologo>();

        foreach (OdontologoRow fila in resultado)
        {
            odontologos.Add(Convertir(fila));
        }

        return odontologos;
    }

    public async Task AgregarOdontologo(Odontologo odontologo)
    {
        if (odontologo == null)
            throw new ArgumentNullException(nameof(odontologo));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
INSERT INTO Odontologos (Id, UsuarioId, Nombre, Matricula, Especialidad, Telefono)
VALUES (@Id, @UsuarioId, @Nombre, @Matricula, @Especialidad, @Telefono);";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(odontologo),
                    _unitOfWork.Transaction));

            if (filas != 1)
                throw new InvalidOperationException($"No se pudo insertar el odontólogo con ID {odontologo.Id}.");

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

    public async Task ActualizarOdontologo(Odontologo odontologo)
    {
        if (odontologo == null)
            throw new ArgumentNullException(nameof(odontologo));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
UPDATE Odontologos
SET UsuarioId = @UsuarioId,
    Nombre = @Nombre,
    Matricula = @Matricula,
    Especialidad = @Especialidad,
    Telefono = @Telefono
WHERE Id = @Id;";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(odontologo),
                    _unitOfWork.Transaction));

            if (filas == 0)
                throw new KeyNotFoundException($"No se encontró un odontólogo con el ID {odontologo.Id}.");

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

    public async Task<bool> EliminarOdontologo(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
DELETE FROM Odontologos
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
FROM Odontologos;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    private static Odontologo Convertir(OdontologoRow fila)
    {
        int id = checked((int)fila.Id);
        int? usuarioId = null;

        if (fila.UsuarioId.HasValue)
            usuarioId = checked((int)fila.UsuarioId.Value);

        string telefono = fila.Telefono ?? string.Empty;

        return new Odontologo(
            id,
            fila.Nombre,
            fila.Matricula,
            fila.Especialidad,
            telefono,
            usuarioId);
    }

    private static object CrearParametros(Odontologo odontologo)
    {
        return new
        {
            Id = odontologo.Id,
            UsuarioId = odontologo.UsuarioId,
            Nombre = odontologo.Nombre,
            Matricula = odontologo.Matricula,
            Especialidad = odontologo.Especialidad,
            Telefono = odontologo.Telefono
        };
    }
}

internal sealed class OdontologoRow
{
    public long Id { get; set; }
    public long? UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
