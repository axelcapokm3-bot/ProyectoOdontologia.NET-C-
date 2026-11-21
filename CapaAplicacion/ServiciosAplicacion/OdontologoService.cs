namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::CapaAplicacion.Mappers;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using CapaDominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OdontologoService : IOdontologoService
{
    private readonly IRepositorioOdontologo _repositorioOdontologo;
    private readonly IRepositorioUsuario? _repositorioUsuario;
    private readonly IPasswordHasher? _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public OdontologoService(
        IRepositorioOdontologo repositorioOdontologo,
        IRepositorioUsuario repositorioUsuario,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _repositorioOdontologo = repositorioOdontologo;
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarOdontologoAsync(OdontologoInputDto odontologo)
    {
        if (!await ValidadorMatricula(odontologo))
        {
            throw new ArgumentException("La matrícula ya está registrada o es inválida.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            int idOdontologo = _repositorioOdontologo.IdAutoIncrementoSincrono();
            int? usuarioId = odontologo.UsuarioId;

            if (usuarioId is null && !string.IsNullOrWhiteSpace(odontologo.NombreUsuario) && !string.IsNullOrWhiteSpace(odontologo.Password))
            {
                usuarioId = await CrearUsuarioOdontologoAsync(odontologo.NombreUsuario, odontologo.Matricula, odontologo.Password);
            }

            Odontologo nuevoOdontologo = new Odontologo(
                idOdontologo,
                odontologo.Nombre,
                odontologo.Matricula,
                odontologo.Especialidad,
                odontologo.Telefono,
                usuarioId);

            await _repositorioOdontologo.AgregarOdontologo(nuevoOdontologo);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<OdontologoOutputDto>> ObtenerTodosLosOdontologosAsync()
    {
        IEnumerable<Odontologo> odontologos = await _repositorioOdontologo.ObtenerTodos();
        List<Odontologo> listaOdontologos = new List<Odontologo>();

        foreach (Odontologo odontologo in odontologos)
        {
            listaOdontologos.Add(odontologo);
        }

        return OdontologoMapper.MapMany(listaOdontologos);
    }

    public async Task<OdontologoOutputDto> ObtenerOdontologoPorIdAsync(int id)
    {
        Odontologo? odontologo = await _repositorioOdontologo.ObtenerOdontologoPorId(id);

        if (odontologo == null)
        {
            throw new OdontologoNoEncontradoException(id);
        }

        return OdontologoMapper.Map(odontologo);
    }

    public async Task<List<OdontologoOutputDto>> BuscarOdontologosAsync(string criterio)
    {
        List<Odontologo> odontologos = await _repositorioOdontologo.BuscarOdontologos(criterio);
        return OdontologoMapper.MapMany(odontologos);
    }

    public async Task ActualizarOdontologoAsync(int id, OdontologoInputDto odontologoDto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            Odontologo? odontologoExistente = await _repositorioOdontologo.ObtenerOdontologoPorId(id);

            if (odontologoExistente == null)
            {
                throw new OdontologoNoEncontradoException(id);
            }

            Odontologo odontologoActualizado = new Odontologo(
                id,
                odontologoDto.Nombre,
                odontologoDto.Matricula,
                odontologoDto.Especialidad,
                odontologoDto.Telefono,
                odontologoDto.UsuarioId);

            await _repositorioOdontologo.ActualizarOdontologo(odontologoActualizado);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> EliminarOdontologoAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            Odontologo? existente = await _repositorioOdontologo.ObtenerOdontologoPorId(id);

            if (existente == null)
            {
                throw new OdontologoNoEncontradoException(id);
            }

            bool eliminado = await _repositorioOdontologo.EliminarOdontologo(id);

            if (!eliminado)
            {
                throw new OdontologoNoEncontradoException(id);
            }

            await _unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ValidadorMatricula(OdontologoInputDto odontologo)
    {
        if (string.IsNullOrWhiteSpace(odontologo.Matricula))
        {
            return false;
        }

        IEnumerable<Odontologo> lista = await _repositorioOdontologo.ObtenerTodos();

        foreach (Odontologo existente in lista)
        {
            if (existente.Matricula.Equals(odontologo.Matricula, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<int> CrearUsuarioOdontologoAsync(string nombreUsuario, string email, string password)
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
            RolUsuario.Odontologo);

        await _repositorioUsuario.AgregarAsync(usuario);

        return usuario.Id;
    }

}
