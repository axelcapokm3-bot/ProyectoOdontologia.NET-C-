namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;


public class RepositorioPaciente : IRepositorioPaciente
{
    private readonly Dictionary<int, Paciente> _pacientes = new();

    public Task<IEnumerable<Paciente>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Paciente>>(_pacientes.Values);
    }

    public Task<Paciente?> ObtenerPacientePorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        _pacientes.TryGetValue(id, out var paciente);
        return Task.FromResult(paciente);
    }

    //sobrecarga de metodo overloading 
    public Task<List<Paciente>> BuscarPacientes(string consulta)
    {

        List<Paciente> resultados = new List<Paciente>();
        if (string.IsNullOrWhiteSpace(consulta))
        {
            foreach (Paciente p in _pacientes.Values)
            {
                resultados.Add(p);
            }
            return Task.FromResult(resultados);



        }

        string busqueda = consulta.Trim().ToLower();

        if (int.TryParse(busqueda, out int idBuscado))
        {
            if (idBuscado >= 0)
            {

                Paciente? pacienteEncontrado = ObtenerPacientePorId(idBuscado).Result;

                if (pacienteEncontrado != null)
                {
                    resultados.Add(pacienteEncontrado);
                    return Task.FromResult(resultados);
                }
            }
        }
        //Busqueda Secundaria Por Nombre si el usuario desea ingresar nombres o apellidos  ; 
        foreach (var paciente in _pacientes.Values)
        {
            string nombre = paciente.Nombre != null ? paciente.Nombre.ToLower() : "";
            string apellido = paciente.Apellido != null ? paciente.Apellido.ToLower() : "";

            if (nombre.Contains(busqueda) || apellido.Contains(busqueda))
            {
                resultados.Add(paciente);
            }
        }

        return Task.FromResult(resultados);
    }





    public Task AgregarPaciente(Paciente paciente)
    {
        if (paciente == null)
            throw new ArgumentNullException(nameof(paciente));

        _pacientes.Add(paciente.Id, paciente);
        return Task.CompletedTask;
    }

    public Task ActualizarPaciente(Paciente paciente)
    {
        if (paciente == null)
            throw new ArgumentNullException(nameof(paciente));

        if (!_pacientes.ContainsKey(paciente.Id))
            throw new KeyNotFoundException($"No se encontró un paciente con el ID {paciente.Id}.");

        _pacientes[paciente.Id] = paciente;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarPaciente(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return Task.FromResult(_pacientes.Remove(id));
    }
}
