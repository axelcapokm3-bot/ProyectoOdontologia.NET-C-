namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioInsumos : IRepositorioInsumos
{
    private readonly ConcurrentDictionary<int, Insumo> _insumos = new();
    private int _contadorId = 0;

    public RepositorioInsumos()
    {
        SincronizarContadorInicial();
    }

    public Task<IEnumerable<Insumo>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Insumo>>(_insumos.Values);
    }

    public Task<Insumo?> ObtenerInsumoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _insumos.TryGetValue(id, out Insumo? insumo);
        return Task.FromResult(insumo);
    }

    public Task<List<Insumo>> BuscarInsumos(string consulta)
    {
        List<Insumo> resultados = new List<Insumo>();

        if (string.IsNullOrWhiteSpace(consulta))
        {
            foreach (Insumo i in _insumos.Values)
            {
                resultados.Add(i);
            }
            return Task.FromResult(resultados);
        }

        string busqueda = consulta.Trim().ToLower();

        // Primera búsqueda: por ID numérico
        if (int.TryParse(busqueda, out int idBuscado))
        {
            if (idBuscado >= 0)
            {
                if (_insumos.TryGetValue(idBuscado, out Insumo? insumoEncontrado))
                {
                    resultados.Add(insumoEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }

    // Segunda búsqueda: por texto en el nombre o categoría
    foreach (Insumo insumo in _insumos.Values)
    {
        string nombre = insumo.Nombre != null ? insumo.Nombre.ToLower() : "";
        string categoria = insumo.Categoria.ToString().ToLower();

        if (nombre.Contains(busqueda) || categoria.Contains(busqueda))
        {
            resultados.Add(insumo);
        }
    }

    return Task.FromResult(resultados);
}

    public Task AgregarInsumo(Insumo insumo)
    {
        if (insumo == null)
            throw new ArgumentNullException(nameof(insumo));

        if (!_insumos.TryAdd(insumo.Id, insumo))
            throw new ArgumentException($"Ya existe un insumo con el ID {insumo.Id}.");

        return Task.CompletedTask;
    }

    public Task ActualizarInsumo(Insumo insumo)
    {
        if (insumo == null)
            throw new ArgumentNullException(nameof(insumo));

        if (!_insumos.ContainsKey(insumo.Id))
            throw new KeyNotFoundException($"No se encontró un insumo con el ID {insumo.Id}.");

        _insumos[insumo.Id] = insumo;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarInsumo(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return Task.FromResult(_insumos.TryRemove(id, out _));
    }

    private void SincronizarContadorInicial()
    {
        int maximoId = 0;

        foreach (int idExistente in _insumos.Keys)
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