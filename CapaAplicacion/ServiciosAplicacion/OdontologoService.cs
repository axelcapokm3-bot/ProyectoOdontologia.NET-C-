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

    public Task RegistrarOdontologoAsync(OdontologoInputDto odontologo)
    {
        if (!ValidadorMatricula(odontologo))
        {
            throw new ArgumentException("La matrícula ya está registrada o es inválida.");
        }

        int nuevoId = autoincremental();

        var nuevoOdontologo = new Odontologo(
            nuevoId,
            odontologo.Nombre,
            odontologo.Matricula,
            odontologo.Especialidad,
            odontologo.Telefono

        );

        _repositorioOdontologo.AgregarOdontologo(nuevoOdontologo);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<OdontologoOutputDto>> ObtenerTodosLosOdontologosAsync()
    {
        var odontologos = _repositorioOdontologo.ObtenerTodos();
        var resultado = new List<OdontologoOutputDto>();

        foreach (var o in odontologos)
        {
            var dto = new OdontologoOutputDto(
                Guid.Parse(o.Id.ToString().PadLeft(32, '0')),
                o.Nombre,
                o.Matricula,
                o.Especialidad,
                o.Telefono
            );
            resultado.Add(dto);
        }

        return Task.FromResult<IEnumerable<OdontologoOutputDto>>(resultado);
    }

    public Task<OdontologoOutputDto> ObtenerOdontologoPorIdAsync(int id)
    {
        var o = _repositorioOdontologo.ObtenerOdontologoPorId(id);
        if (o == null) throw new KeyNotFoundException($"No se encontró el odontólogo con ID {id}");

        var dto = new OdontologoOutputDto(
            Guid.Parse(o.Id.ToString().PadLeft(32, '0')),
            o.Nombre,
            o.Matricula,
            o.Especialidad,
            o.Telefono
        );

        return Task.FromResult<OdontologoOutputDto>(dto);
    }

    public async Task ActualizarOdontologoAsync(int id, OdontologoInputDto odontologoDto)
    {
        // 1. Verificamos existencia
        var odontologoExistente = _repositorioOdontologo.ObtenerOdontologoPorId(id);

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

        // 3. Persistimos los cambios
        _repositorioOdontologo.ActualizarOdontologo(odontologoActualizado);

        await Task.CompletedTask;
    }
    public Task<bool> EliminarOdontologoAsync(int id)
    {
        var existe = _repositorioOdontologo.ObtenerOdontologoPorId(id);
        if (existe == null) return Task.FromResult(false);

        _repositorioOdontologo.EliminarOdontologo(id);
        return Task.FromResult(true);
    }

    public bool ValidadorMatricula(OdontologoInputDto odontologo)
    {
        if (string.IsNullOrWhiteSpace(odontologo.Matricula)) return false;

        var lista = _repositorioOdontologo.ObtenerTodos();
        foreach (var o in lista)
        {
            if (o.Matricula.Equals(odontologo.Matricula, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    public int autoincremental()
    {
        var lista = _repositorioOdontologo.ObtenerTodos();
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