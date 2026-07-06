namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class TratamientoService : ITratamientoService
{
    private readonly IRepositorioTratamiento _repositorioTratamiento;

    public TratamientoService(IRepositorioTratamiento repositorioTratamiento)
    {
        _repositorioTratamiento = repositorioTratamiento;
    }

    public Task RegistrarTratamientoAsync(TratamientoDtoInput tratamiento)
    {
        if (string.IsNullOrWhiteSpace(tratamiento.Descripcion))
        {
            throw new ArgumentException("La descripción del tratamiento no puede estar vacía.");
        }

        if (tratamiento.Costo < 0)
        {
            throw new ArgumentException("El costo no puede ser negativo.");
        }

        int nuevoId = autoincremental();

        var nuevoTratamiento = new Tratamiento(
            nuevoId,
            tratamiento.Descripcion,
            tratamiento.Costo
        );

        _repositorioTratamiento.AgregarTratamiento(nuevoTratamiento);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TratamientoDtoOutput>> ObtenerTodosLosTratamientosAsync()
    {
        var tratamientos = _repositorioTratamiento.ObtenerTodos();
        var resultado = new List<TratamientoDtoOutput>();

        foreach (var t in tratamientos)
        {
            var dto = new TratamientoDtoOutput(
                Guid.Parse(t.Id.ToString().PadLeft(32, '0')),
                t.Descripcion,
                t.Costo
            );
            resultado.Add(dto);
        }

        return Task.FromResult<IEnumerable<TratamientoDtoOutput>>(resultado);
    }

    public Task<TratamientoDtoOutput> ObtenerTratamientoPorIdAsync(int id)
    {
        var t = _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (t == null) throw new KeyNotFoundException($"No se encontró el tratamiento con ID {id}");

        var dto = new TratamientoDtoOutput(
            Guid.Parse(t.Id.ToString().PadLeft(32, '0')),
            t.Descripcion,
            t.Costo
        );

        return Task.FromResult<TratamientoDtoOutput>(dto);
    }

    public Task ActualizarTratamientoAsync(int id, TratamientoDtoInput tratamientoEditado)
    {
        var existe = _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (existe == null) throw new KeyNotFoundException($"No se encontró el tratamiento con ID {id}");

        var tratamientoModificado = new Tratamiento(
            id,
            tratamientoEditado.Descripcion,
            tratamientoEditado.Costo
        );

        _repositorioTratamiento.ActualizarTratamiento(tratamientoModificado);

        return Task.CompletedTask;
    }

    public Task<bool> EliminarTratamientoAsync(int id)
    {
        var existe = _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (existe == null) return Task.FromResult(false);

        _repositorioTratamiento.EliminarTratamiento(id);
        return Task.FromResult(true);
    }

    public int autoincremental()
    {
        var lista = _repositorioTratamiento.ObtenerTodos();
        int idMax = 0;

        foreach (var t in lista)
        {
            if (t.Id > idMax)
            {
                idMax = t.Id;
            }
        }

        return idMax + 1;
    }
}
