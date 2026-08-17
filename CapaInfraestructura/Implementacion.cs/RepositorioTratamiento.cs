namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioTratamiento : IRepositorioTratamiento
{
    private readonly Dictionary<int, Tratamiento> _tratamientos = new();

    public Task<IEnumerable<Tratamiento>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Tratamiento>>(_tratamientos.Values);
    }

    public Task<Tratamiento?> ObtenerTratamientoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _tratamientos.TryGetValue(id, out var tratamiento);
        return Task.FromResult(tratamiento);
    }

    //Sobrecarga de metodo: busqueda hibrida por ID o por texto
    public Task<List<Tratamiento>> BuscarTratamientos(string consulta)
    {
        List<Tratamiento> resultados = new List<Tratamiento>();

        if (string.IsNullOrWhiteSpace(consulta))
        {
            foreach (Tratamiento t in _tratamientos.Values)
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
                Tratamiento? tratamientoEncontrado = ObtenerTratamientoPorId(idBuscado).Result;

                if (tratamientoEncontrado != null)
                {
                    resultados.Add(tratamientoEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }

        //Segunda busqueda: por texto en la Descripcion
        foreach (Tratamiento tratamiento in _tratamientos.Values)
        {
            string descripcion = tratamiento.Descripcion != null ? tratamiento.Descripcion.ToLower() : "";

            if (descripcion.Contains(busqueda))
            {
                resultados.Add(tratamiento);
            }
        }

        return Task.FromResult(resultados);
    }

    public Task AgregarTratamiento(Tratamiento tratamiento)
    {
        if (tratamiento == null)
            throw new ArgumentNullException(nameof(tratamiento));

        _tratamientos.Add(tratamiento.Id, tratamiento);
        return Task.CompletedTask;
    }

    public Task ActualizarTratamiento(Tratamiento tratamiento)
    {
        if (tratamiento == null)
            throw new ArgumentNullException(nameof(tratamiento));

        if (!_tratamientos.ContainsKey(tratamiento.Id))
            throw new KeyNotFoundException($"No se encontró un tratamiento con el ID {tratamiento.Id}.");

        _tratamientos[tratamiento.Id] = tratamiento;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarTratamiento(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return Task.FromResult(_tratamientos.Remove(id));
    }
}
