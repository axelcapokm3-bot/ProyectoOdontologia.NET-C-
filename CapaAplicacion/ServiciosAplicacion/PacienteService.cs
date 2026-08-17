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

    public async Task RegistrarPacienteAsync(PacienteDtoInput paciente)
    {
        if (!ValidadorNombre(paciente))
        {
            throw new ArgumentException("El nombre del paciente contiene caracteres no válidos o está vacío.");
        }

        int nuevoId = await autoincremental();

        var nuevoPaciente = new Paciente(
            nuevoId,
            paciente.Nombre,
            paciente.Apellido,
            paciente.FechaNacimiento,
            paciente.Telefono,
            paciente.Email
        );

        if (!await ValidadorDeEmail(nuevoPaciente))
        {
            throw new InvalidOperationException($"El correo electrónico '{paciente.Email}' ya está registrado.");
        }

        if (!await ValidarTelefono(nuevoPaciente))
        {
            throw new InvalidOperationException($"El teléfono '{paciente.Telefono}' ya está registrado por otro paciente.");
        }

        await _repositorioPaciente.AgregarPaciente(nuevoPaciente);
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

    public async Task<bool> ValidadorDeEmail(Paciente paciente)
    {
        var pacientes = await _repositorioPaciente.ObtenerTodos();

        foreach (var p in pacientes)
        {
            if (paciente.Id != p.Id)
            {
                if (paciente.Email.Equals(p.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> ValidarTelefono(Paciente paciente)
    {
        var pacientes = await _repositorioPaciente.ObtenerTodos();

        foreach (var p in pacientes)
        {
            if (paciente.Id != p.Id)
            {
                if (p.Telefono.Equals(paciente.Telefono))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public async Task<IEnumerable<PacienteDtoOutput>> ObtenerTodosLosPacientesAsync()
    {
        var pacientes = await _repositorioPaciente.ObtenerTodos();

        var resultado = new List<PacienteDtoOutput>();

        foreach (var p in pacientes)
        {
            var dto = new PacienteDtoOutput(
                p.Id,
                p.Nombre,
                p.Apellido,
                p.Email,
                p.FechaNacimiento,
                p.Telefono
            );

            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task<List<PacienteDtoOutput>> BuscarPacientesAsync(string criterio)
    {
        List<Paciente> pacientes = await _repositorioPaciente.BuscarPacientes(criterio);
        List<PacienteDtoOutput> resultado = new List<PacienteDtoOutput>();

        foreach (Paciente p in pacientes)
        {
            var dto = new PacienteDtoOutput(
                p.Id,
                p.Nombre,
                p.Apellido,
                p.Email,
                p.FechaNacimiento,
                p.Telefono
            );
            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task<PacienteDtoOutput> ObtenerPacientePorIdAsync(int id)
    {
        var p = await _repositorioPaciente.ObtenerPacientePorId(id);
        if (p == null) throw new KeyNotFoundException($"No se encontró el paciente con ID {id}");

        var dto = new PacienteDtoOutput(
            p.Id,
            p.Nombre,
            p.Apellido,
            p.Email,
            p.FechaNacimiento,
            p.Telefono
        );

        return dto;
    }

    public async Task ActualizarPacienteAsync(int id, PacienteDtoInput pacienteEditado)
    {
        var existe = await _repositorioPaciente.ObtenerPacientePorId(id);
        if (existe == null) throw new KeyNotFoundException($"No se encontró el paciente con ID {id}");

        var pacienteModificado = new Paciente(
            id,
            pacienteEditado.Nombre,
            pacienteEditado.Apellido,
            pacienteEditado.FechaNacimiento,
            pacienteEditado.Telefono,
            pacienteEditado.Email
        );

        await _repositorioPaciente.ActualizarPaciente(pacienteModificado);
    }

    public async Task<bool> EliminarPacienteAsync(int id)
    {
        var existe = await _repositorioPaciente.ObtenerPacientePorId(id);
        if (existe == null) return false;

        await _repositorioPaciente.EliminarPaciente(id);
        return true;
    }

    public async Task<int> autoincremental()
    {
        var lista = await _repositorioPaciente.ObtenerTodos();

        int idMax = 0;

        foreach (var p in lista)
        {
            if (p.Id > idMax)
            {
                idMax = p.Id;
            }
        }

        return idMax + 1;
    }
}
