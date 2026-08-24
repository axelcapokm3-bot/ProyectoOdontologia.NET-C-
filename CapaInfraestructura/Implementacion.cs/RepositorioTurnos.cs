namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioTurnos : IRepositorioTurnos
{
    private readonly ConcurrentDictionary<int, Turno> _turnos = new();
    private int _contadorId = 0;

    public RepositorioTurnos()
    {
        SincronizarContadorInicial();
    }

    public Task<IEnumerable<Turno>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Turno>>(_turnos.Values);
    }

    public Task<Turno?> ObtenerTurnoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _turnos.TryGetValue(id, out var turno);
        return Task.FromResult(turno);
    }

    //Sobrecarga de metodo: busqueda hibrida por ID o por texto
    public Task<List<Turno>> BuscarTurnos(string consulta)
    {
        List<Turno> resultados = new List<Turno>();

        if (string.IsNullOrWhiteSpace(consulta))
        {
            foreach (Turno t in _turnos.Values)
            {
                resultados.Add(t);
            }
            return Task.FromResult(resultados);
        }

        string busqueda = consulta.Trim().ToLower();

        //Primera busqueda: por ID numerico
        if (int.TryParse(busqueda, out int idBuscado))
        {
            if (idBuscado >= 0)
            {
                if (_turnos.TryGetValue(idBuscado, out Turno? turnoEncontrado))
                {
                    resultados.Add(turnoEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }

        //Segunda busqueda: por texto en FechaHora o por IDs de paciente/odontologo/tratamiento
        foreach (Turno turno in _turnos.Values)
        {
            string fechaHora = turno.FechaHora.ToString("dd/MM/yyyy HH:mm");
            string pacienteId = turno.PacienteId.ToString();
            string odontologoId = turno.OdontologoId.ToString();
            string tratamientoId = turno.TratamientoId.ToString();

            if (fechaHora.Contains(busqueda) || pacienteId.Contains(busqueda) ||
                odontologoId.Contains(busqueda) || tratamientoId.Contains(busqueda))
            {
                resultados.Add(turno);
            }
        }

        return Task.FromResult(resultados);
    }

    public Task AgregarTurno(Turno turno)
    {
        if (turno == null)
            throw new ArgumentNullException(nameof(turno));

        if (!_turnos.TryAdd(turno.Id, turno))
            throw new ArgumentException($"Ya existe un turno con el ID {turno.Id}.");

        return Task.CompletedTask;
    }

    public Task ActualizarTurno(Turno turno)
    {
        if (turno == null)
            throw new ArgumentNullException(nameof(turno));

        if (!_turnos.ContainsKey(turno.Id))
            throw new KeyNotFoundException($"No se encontró un turno con el ID {turno.Id}.");

        _turnos[turno.Id] = turno;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarTurno(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return Task.FromResult(_turnos.TryRemove(id, out _));
    }

    private void SincronizarContadorInicial()
    {
        int maximoId = 0;

        foreach (int idExistente in _turnos.Keys)
        {
            if (idExistente > maximoId)
                maximoId = idExistente;
        }

        _contadorId = maximoId;
    }

    public int IdAutoIncrementoSincrono()
    {
        return Interlocked.Increment(ref _contadorId);
    }
}
