namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioOdontologo : IRepositorioOdontologo
{
    private static List<Odontologo> _odontologos = new List<Odontologo>();

    public RepositorioOdontologo()
    {
        _odontologos = new List<Odontologo>();
    }

    public List<Odontologo> ObtenerTodos()
    {
        return _odontologos;
    }

    public Odontologo? ObtenerOdontologoPorId(int id)
    {
        foreach (var o in _odontologos)
        {
            if (o.Id == id)
            {
                return o;
            }
        }
        return null;
    }

    public void AgregarOdontologo(Odontologo odontologo)
    {
        _odontologos.Add(odontologo);
    }

    public void ActualizarOdontologo(Odontologo odontologo)
    {
        if (odontologo == null)
        {

        }

        foreach (Odontologo i in _odontologos)
        {
            if (i.Id == odontologo.Id)
            {
                i.Nombre = odontologo.Nombre;
                i.Matricula = odontologo.Matricula;
                i.Especialidad = odontologo.Especialidad;
                i.Telefono = odontologo.Telefono;
            }
        }

        return;
    }

    public void EliminarOdontologo(int id)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");
        }

        for (int i = 0; i < _odontologos.Count; i++)
        {
            if (_odontologos[i].Id == id)
            {
                _odontologos.RemoveAt(i);
            }
        }

        return;
    }
}
