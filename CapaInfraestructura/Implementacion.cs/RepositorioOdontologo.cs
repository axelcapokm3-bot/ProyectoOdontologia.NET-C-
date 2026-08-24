namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioOdontologo : IRepositorioOdontologo
{
    private readonly ConcurrentDictionary<int, Odontologo> _odontologos = new();
    private int _contadorId = 0;

    public RepositorioOdontologo()
    {
        SincronizarContadorInicial();
    }

    public Task<IEnumerable<Odontologo>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Odontologo>>(_odontologos.Values);
    }

    public Task<Odontologo?> ObtenerOdontologoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _odontologos.TryGetValue(id, out var odontologo);
        return Task.FromResult(odontologo);
    }

    //Sobrecarga de metodo: busqueda hibrida por ID o por texto
    public Task<List<Odontologo>> BuscarOdontologos(string consulta)
    {
        List<Odontologo> resultados = new List<Odontologo>();

        if (string.IsNullOrWhiteSpace(consulta))
        {
            foreach (Odontologo o in _odontologos.Values)
            {
                resultados.Add(o);
            }
            return Task.FromResult(resultados);
        }

        string busqueda = consulta.Trim().ToLower();

        //Primera busqueda: por ID numerico
        if (int.TryParse(busqueda, out int idBuscado))
        {
            if (idBuscado >= 0)
            {
                if (_odontologos.TryGetValue(idBuscado, out Odontologo? odontologoEncontrado))
                {
                    resultados.Add(odontologoEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }

        //Segunda busqueda: por texto en Nombre, Matricula o Especialidad
        foreach (Odontologo odontologo in _odontologos.Values)
        {
            string nombre = odontologo.Nombre != null ? odontologo.Nombre.ToLower() : "";
            string matricula = odontologo.Matricula != null ? odontologo.Matricula.ToLower() : "";
            string especialidad = odontologo.Especialidad != null ? odontologo.Especialidad.ToLower() : "";

            if (nombre.Contains(busqueda) || matricula.Contains(busqueda) || especialidad.Contains(busqueda))
            {
                resultados.Add(odontologo);
            }
        }

        return Task.FromResult(resultados);
    }

    public Task AgregarOdontologo(Odontologo odontologo)
    {
        if (odontologo == null)
            throw new ArgumentNullException(nameof(odontologo));

        if (!_odontologos.TryAdd(odontologo.Id, odontologo))
            throw new ArgumentException($"Ya existe un odontólogo con el ID {odontologo.Id}.");

        return Task.CompletedTask;
    }

    public Task ActualizarOdontologo(Odontologo odontologo)
    {
        if (odontologo == null)
            throw new ArgumentNullException(nameof(odontologo));

        if (!_odontologos.ContainsKey(odontologo.Id))
            throw new KeyNotFoundException($"No se encontró un odontólogo con el ID {odontologo.Id}.");

        _odontologos[odontologo.Id] = odontologo;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarOdontologo(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return Task.FromResult(_odontologos.TryRemove(id, out _));
    }

    private void SincronizarContadorInicial()
    {
        int maximoId = 0;

        foreach (int idExistente in _odontologos.Keys)
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
