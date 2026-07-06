namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioTurnos : IRepositorioTurnos
{
    private static List<Turnos> _turnos = new List<Turnos>();

    public RepositorioTurnos()
    {
        _turnos = new List<Turnos>();
    }

    public List<Turnos> ObtenerTodos()
    {
        return _turnos;
    }

    public Turnos? ObtenerTurnoPorId(int id)
    {
        foreach (var t in _turnos)
        {
            if (t.Id == id)
            {
                return t;
            }
        }
        return null;
    }

    public void AgregarTurno(Turnos turno)
    {
        _turnos.Add(turno);
    }

    public void ActualizarTurno(Turnos turno)
    {
        if (turno == null)
        {

        }

        foreach (Turnos i in _turnos)
        {
            if (i.Id == turno.Id)
            {
                i.FechaHora = turno.FechaHora;
                i.PacienteId = turno.PacienteId;
                i.OdontologoId = turno.OdontologoId;
                i.TratamientoId = turno.TratamientoId;
            }
        }

        return;
    }

    public void EliminarTurno(int id)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");
        }

        for (int i = 0; i < _turnos.Count; i++)
        {
            if (_turnos[i].Id == id)
            {
                _turnos.RemoveAt(i);
            }
        }

        return;
    }
}
