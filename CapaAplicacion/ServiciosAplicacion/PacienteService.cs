namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using CapaDominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PacienteService : IPacienteService
{
    private readonly IRepositorioPaciente _repositorioPaciente;
    private readonly IRepositorioUsuario? _repositorioUsuario;
    private readonly IPasswordHasher? _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public PacienteService(
        IRepositorioPaciente repositorioPaciente,
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _repositorioPaciente = repositorioPaciente;
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarPacienteAsync(PacienteDtoInput paciente)
    {
        if (!ValidadorNombre(paciente))
        {
            throw new ArgumentException("El nombre del paciente contiene caracteres no válidos o está vacío.");
        }

        await _unitOfWork.BeginTransactionAsync();

        int idPaciente = _repositorioPaciente.IdAutoIncrementoSincrono();
        int? usuarioId = paciente.UsuarioId;

        if (usuarioId is null && !string.IsNullOrWhiteSpace(paciente.NombreUsuario) && !string.IsNullOrWhiteSpace(paciente.Password))
        {
            usuarioId = await CrearUsuarioPacienteAsync(paciente.NombreUsuario, paciente.Email, paciente.Password);
        }

        Paciente nuevoPaciente = new Paciente(
            idPaciente,
            paciente.Nombre,
            paciente.Apellido,
            paciente.FechaNacimiento,
            paciente.Telefono,
            paciente.Email,
            usuarioId);

        if (!await ValidadorDeEmail(nuevoPaciente))
        {
            throw new InvalidOperationException($"El correo electrónico '{paciente.Email}' ya está registrado.");
        }

        if (!await ValidarTelefono(nuevoPaciente))
        {
            throw new InvalidOperationException($"El teléfono '{paciente.Telefono}' ya está registrado por otro paciente.");
        }

        await _repositorioPaciente.AgregarPaciente(nuevoPaciente);
        await _unitOfWork.CommitAsync();
    }

    public bool ValidadorNombre(PacienteDtoInput paciente)
    {
        if (string.IsNullOrWhiteSpace(paciente.Nombre))
        {
            return false;
        }

        foreach (char caracter in paciente.Nombre)
        {
            if (!char.IsLetter(caracter) && !char.IsWhiteSpace(caracter))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> ValidadorDeEmail(Paciente paciente)
    {
        IEnumerable<Paciente> pacientes = await _repositorioPaciente.ObtenerTodos();

        foreach (Paciente existente in pacientes)
        {
            if (paciente.Id != existente.Id)
            {
                if (paciente.Email.Equals(existente.Email, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> ValidarTelefono(Paciente paciente)
    {
        IEnumerable<Paciente> pacientes = await _repositorioPaciente.ObtenerTodos();

        foreach (Paciente existente in pacientes)
        {
            if (paciente.Id != existente.Id)
            {
                if (existente.Telefono.Equals(paciente.Telefono))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<IEnumerable<PacienteDtoOutput>> ObtenerTodosLosPacientesAsync()
    {
        IEnumerable<Paciente> pacientes = await _repositorioPaciente.ObtenerTodos();
        List<PacienteDtoOutput> resultado = new List<PacienteDtoOutput>();

        foreach (Paciente paciente in pacientes)
        {
            resultado.Add(CrearDto(paciente));
        }

        return resultado;
    }

    public async Task<List<PacienteDtoOutput>> BuscarPacientesAsync(string criterio)
    {
        List<Paciente> pacientes = await _repositorioPaciente.BuscarPacientes(criterio);
        List<PacienteDtoOutput> resultado = new List<PacienteDtoOutput>();

        foreach (Paciente paciente in pacientes)
        {
            resultado.Add(CrearDto(paciente));
        }

        return resultado;
    }

    public async Task<PacienteDtoOutput> ObtenerPacientePorIdAsync(int id)
    {
        Paciente? paciente = await _repositorioPaciente.ObtenerPacientePorId(id);

        if (paciente == null)
        {
            throw new PacienteNoEncontradoException(id);
        }

        return CrearDto(paciente);
    }

    public async Task ActualizarPacienteAsync(int id, PacienteDtoInput pacienteEditado)
    {
        await _unitOfWork.BeginTransactionAsync();

        Paciente pacienteModificado = new Paciente(
            id,
            pacienteEditado.Nombre,
            pacienteEditado.Apellido,
            pacienteEditado.FechaNacimiento,
            pacienteEditado.Telefono,
            pacienteEditado.Email,
            pacienteEditado.UsuarioId);

        if (!await ValidadorDeEmail(pacienteModificado))
        {
            throw new InvalidOperationException($"El correo electrónico '{pacienteEditado.Email}' ya está registrado.");
        }

        if (!await ValidarTelefono(pacienteModificado))
        {
            throw new InvalidOperationException($"El teléfono '{pacienteEditado.Telefono}' ya está registrado por otro paciente.");
        }

        Paciente? existente = await _repositorioPaciente.ObtenerPacientePorId(id);

        if (existente == null)
        {
            throw new PacienteNoEncontradoException(id);
        }

        await _repositorioPaciente.ActualizarPaciente(pacienteModificado);
        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> EliminarPacienteAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();

        Paciente? existente = await _repositorioPaciente.ObtenerPacientePorId(id);

        if (existente == null)
        {
            throw new PacienteNoEncontradoException(id);
        }

        bool eliminado = await _repositorioPaciente.EliminarPaciente(id);

        if (!eliminado)
        {
            throw new PacienteNoEncontradoException(id);
        }

        await _unitOfWork.CommitAsync();

        return true;
    }

    private async Task<int> CrearUsuarioPacienteAsync(string nombreUsuario, string email, string password)
    {
        if (_repositorioUsuario is null || _passwordHasher is null)
        {
            throw new InvalidOperationException("El registro automático de usuario no está configurado.");
        }

        if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(nombreUsuario) is not null)
        {
            throw new InvalidOperationException("El nombre de usuario ya existe.");
        }

        if (await _repositorioUsuario.ObtenerPorEmailAsync(email) is not null)
        {
            throw new InvalidOperationException("El email de usuario ya existe.");
        }

        Usuario usuario = new Usuario(
            _repositorioUsuario.IdAutoIncrementoSincrono(),
            nombreUsuario,
            email,
            _passwordHasher.Hash(password),
            RolUsuario.Paciente);

        await _repositorioUsuario.AgregarAsync(usuario);

        return usuario.Id;
    }

    private static PacienteDtoOutput CrearDto(Paciente paciente)
    {
        return new PacienteDtoOutput(
            paciente.Id,
            paciente.Nombre,
            paciente.Apellido,
            paciente.Email,
            paciente.FechaNacimiento,
            paciente.Telefono);
    }
}
