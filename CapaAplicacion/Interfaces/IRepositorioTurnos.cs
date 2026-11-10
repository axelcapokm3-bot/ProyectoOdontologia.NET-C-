namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IRepositorioTurnos
{

    Task<IEnumerable<Turno>> ObtenerTodos();
    Task<Turno?> ObtenerTurnoPorId(int id);

    Task<List<Turno>> BuscarTurnos(string criterio);


    Task AgregarTurno(Turno turno);


    Task ActualizarTurno(Turno turno);

    Task<bool> EliminarTurno(int id);

    int IdAutoIncrementoSincrono();

    Task<Turno?> ObtenerTurnoPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Turno>> ObtenerTurnosDelDiaAsync(int odontologoId, DateTime fecha, CancellationToken cancellationToken = default);

    Task<int> ObtenerProximoIdAsync(CancellationToken cancellationToken = default);

    Task<int> AgregarTurnoAsync(Turno turno, CancellationToken cancellationToken = default);

    Task ActualizarTurnoAsync(Turno turno, CancellationToken cancellationToken = default);
}
