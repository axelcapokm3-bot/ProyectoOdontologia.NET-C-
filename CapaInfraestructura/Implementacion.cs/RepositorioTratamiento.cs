namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioTratamiento : IRepositorioTratamiento
{
    private static List<Tratamiento> _tratamientos = new List<Tratamiento>();

    public RepositorioTratamiento()
    {
        _tratamientos = new List<Tratamiento>();
    }

    public List<Tratamiento> ObtenerTodos()
    {
        return _tratamientos;
    }

    public Tratamiento? ObtenerTratamientoPorId(int id)
    {
        foreach (var t in _tratamientos)
        {
            if (t.Id == id)
            {
                return t;
            }
        }
        return null;
    }

    public void AgregarTratamiento(Tratamiento tratamiento)
    {
        _tratamientos.Add(tratamiento);
    }

    public void ActualizarTratamiento(Tratamiento tratamiento)
    {
        if (tratamiento == null)
        {

        }

        foreach (Tratamiento i in _tratamientos)
        {
            if (i.Id == tratamiento.Id)
            {
                i.Descripcion = tratamiento.Descripcion;
                i.Costo = tratamiento.Costo;
            }
        }

        return;
    }

    public void EliminarTratamiento(int id)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");
        }

        for (int i = 0; i < _tratamientos.Count; i++)
        {
            if (_tratamientos[i].Id == id)
            {
                _tratamientos.RemoveAt(i);
            }
        }

        return;
    }
}
