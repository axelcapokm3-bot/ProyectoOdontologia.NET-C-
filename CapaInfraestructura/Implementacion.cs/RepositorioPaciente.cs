namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioPaciente : IRepositorioPaciente
{
    private static List<Paciente> _pacientes = new List<Paciente>();




    public List<Paciente> ObtenerTodos()
    {
        return _pacientes;
    }


    public Paciente? ObtenerPacientePorId(int id)
    {
        foreach (var p in _pacientes)
        {
            if (p.Id == id)
            {
                return p;
            }
        }
        return null;

    }


    public void AgregarPaciente(Paciente paciente)
    {

        _pacientes.Add(paciente);
    }

    // 4. Actualizar Paciente
    public void ActualizarPaciente(Paciente paciente)
    {
        if (paciente == null)
        {

        }

        foreach (Paciente i in _pacientes)
        {
            if (i.Id == paciente.Id)
            {
                i.Nombre = paciente.Nombre;
                i.Apellido = paciente.Apellido;
                i.Email = paciente.Email;
                i.FechaNacimiento = paciente.FechaNacimiento;
                i.Telefono = paciente.Telefono;

            }
        }

        return;
    }

    // 5. Eliminar Paciente
    public void EliminarPaciente(int id)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");
        }


        for (int i = 0; i < _pacientes.Count; i++)
        {
            if (_pacientes[i].Id == id)
            {
                _pacientes.RemoveAt(i);
            }

        }
        return;
    }
}