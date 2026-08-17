namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioInsumos : IRepositorioInsumos
{
    private readonly Dictionary<int, Insumo> _insumos = new();

    public Task<IEnumerable<Insumo>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Insumo>>(_insumos.Values);
    }

    public Task<Insumo?> ObtenerInsumoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _insumos.TryGetValue(id, out var insumo);
        return Task.FromResult(insumo);
    }

    //Busqueda hibrida por ID numerico o por texto en el nombre
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

        //Primera busqueda: por ID numerico
        if (int.TryParse(busqueda, out int idBuscado))
        {
            if (idBuscado >= 0)
            {
                Insumo? insumoEncontrado = ObtenerInsumoPorId(idBuscado).Result;

                if (insumoEncontrado != null)
                {
                    resultados.Add(insumoEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }

        //Segunda busqueda: por texto en el nombre o categoria
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

        _insumos.Add(insumo.Id, insumo);
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

        return Task.FromResult(_insumos.Remove(id));
    }
}
