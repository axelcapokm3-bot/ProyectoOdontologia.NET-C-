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

    public OdontologoService(IRepositorioOdontologo repositorioOdontologo)
    {
        _repositorioOdontologo = repositorioOdontologo;
    }

    public async Task RegistrarOdontologoAsync(OdontologoInputDto odontologo)
    {
        if (!await ValidadorMatricula(odontologo))
        {
            throw new ArgumentException("La matrícula ya está registrada o es inválida.");
        }

        int nuevoId = await autoincremental();

        var nuevoOdontologo = new Odontologo(
            nuevoId,
            odontologo.Nombre,
            odontologo.Matricula,
            odontologo.Especialidad,
            odontologo.Telefono
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
            odontologoDto.Telefono
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

    public async Task<int> autoincremental()
    {
        var lista = await _repositorioOdontologo.ObtenerTodos();
        int idMax = 0;

        foreach (var o in lista)
        {
            if (o.Id > idMax)
            {
                idMax = o.Id;
            }
        }

        return idMax + 1;
    }
}
