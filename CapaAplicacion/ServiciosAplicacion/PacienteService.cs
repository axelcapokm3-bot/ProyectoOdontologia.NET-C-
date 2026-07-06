namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class PacienteService : IPacienteService
{
    private readonly IRepositorioPaciente _repositorioPaciente;

    public PacienteService(IRepositorioPaciente repositorioPaciente)
    {
        _repositorioPaciente = repositorioPaciente;
    }

    public Task RegistrarPacienteAsync(PacienteDtoInput paciente)
    {

        if (!ValidadorNombre(paciente))
        {
            throw new ArgumentException("El nombre del paciente contiene caracteres no válidos o está vacío.");
        }

        int nuevoId = autoincremental();

        var nuevoPaciente = new Paciente(
            nuevoId,
            paciente.Nombre,
            paciente.Apellido,
            paciente.FechaNacimiento,
            paciente.Telefono,
            paciente.Email

        );

        // 2. Ejecutar validaciones lógicas cruzadas de identidad y contacto
        if (!ValidadorDeEmail(nuevoPaciente))
        {
            throw new InvalidOperationException($"El correo electrónico '{paciente.Email}' ya está registrado.");
        }

        if (!ValidarTelefono(nuevoPaciente))
        {
            throw new InvalidOperationException($"El teléfono '{paciente.Telefono}' ya está registrado por otro paciente.");
        }



        _repositorioPaciente.AgregarPaciente(nuevoPaciente);

        return Task.CompletedTask;
    }


    public bool ValidadorNombre(PacienteDtoInput paciente)
    {
        if (string.IsNullOrWhiteSpace(paciente.Nombre)) return false;

        foreach (char Nombre in paciente.Nombre)
        {
            if (!char.IsLetter(Nombre) && !char.IsWhiteSpace(Nombre))
            {
                return false;
            }
        }

        return true;
    }

    public bool ValidadorDeEmail(Paciente paciente)
    {
        var Pacientes = _repositorioPaciente.ObtenerTodos();

        foreach (var P in Pacientes)
        {
            if (paciente.Id != P.Id)
            {

                if (paciente.Email.Equals(P.Email, StringComparison.OrdinalIgnoreCase))
                {

                    return false;
                }

            }
        }

        return true;

    }

    public bool ValidarTelefono(Paciente paciente)
    {
        var pacientes = _repositorioPaciente.ObtenerTodos();

        foreach (var P in pacientes)
        {
            if (paciente.Id != P.Id)
            {
                if (P.Telefono.Equals(paciente.Telefono))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public Task<IEnumerable<PacienteDtoOutput>> ObtenerTodosLosPacientesAsync()
    {
        var pacientes = _repositorioPaciente.ObtenerTodos();


        var resultado = new List<PacienteDtoOutput>();

        foreach (var p in pacientes)
        {
            var dto = new PacienteDtoOutput(
                Guid.Parse(p.Id.ToString().PadLeft(32, '0')),
                p.Nombre,
                p.Apellido,
                p.Email,
                p.FechaNacimiento,
                p.Telefono
            );

            resultado.Add(dto); // CORRECCIÓN: Guardamos el DTO explícitamente en la lista antes de terminar la vuelta
        }

        // Materializamos la lista en memoria

        return Task.FromResult<IEnumerable<PacienteDtoOutput>>(resultado);
    }

    public Task<PacienteDtoOutput> ObtenerPacientePorIdAsync(int id)
    {
        var p = _repositorioPaciente.ObtenerPacientePorId(id);
        if (p == null) throw new KeyNotFoundException($"No se encontró el paciente con ID {id}");

        var dto = new PacienteDtoOutput(
             Guid.Parse(p.Id.ToString().PadLeft(32, '0')),
            p.Nombre,
            p.Apellido,
            p.Email,
            p.FechaNacimiento,
            p.Telefono
        );

        return Task.FromResult<PacienteDtoOutput>(dto);
    }

    public Task ActualizarPacienteAsync(int id, PacienteDtoInput pacienteEditado)
    {
        var existe = _repositorioPaciente.ObtenerPacientePorId(id);
        if (existe == null) throw new KeyNotFoundException($"No se encontró el paciente con ID {id}");

        var pacienteModificado = new Paciente(
            id,
            pacienteEditado.Nombre,
            pacienteEditado.Apellido,
            pacienteEditado.FechaNacimiento,
            pacienteEditado.Telefono,
            pacienteEditado.Email
        );

        _repositorioPaciente.ActualizarPaciente(pacienteModificado);

        return Task.CompletedTask;
    }

    public Task<bool> EliminarPacienteAsync(int id)
    {
        // Corrección: Buscamos si el paciente específico existe por ID antes de intentar borrarlo
        var existe = _repositorioPaciente.ObtenerPacientePorId(id);
        if (existe == null) return Task.FromResult(false);

        _repositorioPaciente.EliminarPaciente(id);
        return Task.FromResult(true);
    }



    //Metodo auxiliar Simulo autoincremental de ID  
    public int autoincremental(int _ = 0)
    {
        var Lista = _repositorioPaciente.ObtenerTodos();

        int IdMax = 0;

        foreach (var P in Lista)
        {
            if (P.Id > IdMax)
            {
                IdMax = P.Id;
            }
        }

        return IdMax + 1;
    }
}