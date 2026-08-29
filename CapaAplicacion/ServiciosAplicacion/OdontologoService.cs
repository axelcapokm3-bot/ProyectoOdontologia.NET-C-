namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class OdontologoService : IOdontologoService
{
    private readonly IRepositorioOdontologo _repositorioOdontologo;
    private readonly IRepositorioUsuario? _repositorioUsuario;
    private readonly IPasswordHasher? _passwordHasher;

    public OdontologoService(IRepositorioOdontologo repositorioOdontologo)
    {
        _repositorioOdontologo = repositorioOdontologo;
    }

    public OdontologoService(IRepositorioOdontologo repositorioOdontologo, IRepositorioUsuario repositorioUsuario, IPasswordHasher passwordHasher)
    {
        _repositorioOdontologo = repositorioOdontologo;
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
    }

    public async Task RegistrarOdontologoAsync(OdontologoInputDto odontologo)
    {
        if (!await ValidadorMatricula(odontologo))
        {
            throw new ArgumentException("La matrícula ya está registrada o es inválida.");
        }

        int idOdontologo = _repositorioOdontologo.IdAutoIncrementoSincrono();
        int? usuarioId = odontologo.UsuarioId;
        if (usuarioId is null && !string.IsNullOrWhiteSpace(odontologo.NombreUsuario) && !string.IsNullOrWhiteSpace(odontologo.Password))
        {
            usuarioId = await CrearUsuarioOdontologoAsync(odontologo.NombreUsuario, odontologo.Matricula, odontologo.Password);
        }

        var nuevoOdontologo = new Odontologo(
            idOdontologo,
            odontologo.Nombre,
            odontologo.Matricula,
            odontologo.Especialidad,
            odontologo.Telefono,
            usuarioId
        );

        await _repositorioOdontologo.AgregarOdontologo(nuevoOdontologo);
    }

    public async Task<IEnumerable<OdontologoOutputDto>> ObtenerTodosLosOdontologosAsync()
    {
        var odontologos = await _repositorioOdontologo.ObtenerTodos();
        var resultado = new List<OdontologoOutputDto>();

        foreach (var o in odontologos)
        {
            var dto = new OdontologoOutputDto(
                o.Id,
                o.Nombre,
                o.Matricula,
                o.Especialidad,
                o.Telefono
            );
            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task<OdontologoOutputDto> ObtenerOdontologoPorIdAsync(int id)
    {
        var o = await _repositorioOdontologo.ObtenerOdontologoPorId(id);
        if (o == null) throw new KeyNotFoundException($"No se encontró el odontólogo con ID {id}");

        var dto = new OdontologoOutputDto(
            o.Id,
            o.Nombre,
            o.Matricula,
            o.Especialidad,
            o.Telefono
        );

        return dto;
    }

    public async Task<List<OdontologoOutputDto>> BuscarOdontologosAsync(string criterio)
    {
        List<Odontologo> odontologos = await _repositorioOdontologo.BuscarOdontologos(criterio);
        List<OdontologoOutputDto> resultado = new List<OdontologoOutputDto>();

        foreach (Odontologo o in odontologos)
        {
            var dto = new OdontologoOutputDto(
                o.Id,
                o.Nombre,
                o.Matricula,
                o.Especialidad,
                o.Telefono
            );
            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task ActualizarOdontologoAsync(int id, OdontologoInputDto odontologoDto)
    {
        var odontologoExistente = await _repositorioOdontologo.ObtenerOdontologoPorId(id);

        if (odontologoExistente == null)
        {
            throw new KeyNotFoundException($"No se encontró el odontólogo con ID {id}");
        }

        var odontologoActualizado = new Odontologo(
            id,
            odontologoDto.Nombre,
            odontologoDto.Matricula,
            odontologoDto.Especialidad,
            odontologoDto.Telefono,
            odontologoDto.UsuarioId
        );

        await _repositorioOdontologo.ActualizarOdontologo(odontologoActualizado);
    }

    public async Task<bool> EliminarOdontologoAsync(int id)
    {
        var existe = await _repositorioOdontologo.ObtenerOdontologoPorId(id);
        if (existe == null) return false;

        await _repositorioOdontologo.EliminarOdontologo(id);
        return true;
    }

    public async Task<bool> ValidadorMatricula(OdontologoInputDto odontologo)
    {
        if (string.IsNullOrWhiteSpace(odontologo.Matricula)) return false;

        var lista = await _repositorioOdontologo.ObtenerTodos();
        foreach (var o in lista)
        {
            if (o.Matricula.Equals(odontologo.Matricula, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    private async Task<int> CrearUsuarioOdontologoAsync(string nombreUsuario, string email, string password)
    {
        if (_repositorioUsuario is null || _passwordHasher is null)
            throw new InvalidOperationException("El registro automático de usuario no está configurado.");

        if (await _repositorioUsuario.ObtenerPorNombreUsuarioAsync(nombreUsuario) is not null)
            throw new InvalidOperationException("El nombre de usuario ya existe.");

        if (await _repositorioUsuario.ObtenerPorEmailAsync(email) is not null)
            throw new InvalidOperationException("El email de usuario ya existe.");

        var usuario = new Usuario(
            _repositorioUsuario.IdAutoIncrementoSincrono(),
            nombreUsuario,
            email,
            _passwordHasher.Hash(password),
            RolUsuario.Odontologo);

        await _repositorioUsuario.AgregarAsync(usuario);
        return usuario.Id;
    }
}
