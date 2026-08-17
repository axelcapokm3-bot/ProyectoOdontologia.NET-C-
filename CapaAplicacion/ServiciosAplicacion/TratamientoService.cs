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
    private readonly IRepositorioInsumos _repositorioInsumos;


    public TratamientoService(IRepositorioTratamiento repositorioTratamiento, IRepositorioInsumos repositorioInsumos)
    {
        _repositorioTratamiento = repositorioTratamiento;
        _repositorioInsumos = repositorioInsumos;
    }
    public async Task RegistrarTratamientoAsync(TratamientoDtoInput tratamiento)
    {
        //validacion de la descripcion si nulo o vacio contiene espacios blanco lanzamos la excpecion 
        if (string.IsNullOrWhiteSpace(tratamiento.Descripcion))
        {
            throw new ArgumentException("La descripción del tratamiento no puede estar vacía.");
        }
        // el costo no debe ser negativo  rompe la regla de negocio y yo lanzo la excepcion 
        if (tratamiento.Costo < 0)
        {
            throw new ArgumentException("El costo no puede ser negativo.");
        }


        int nuevoId = await autoincremental();

        var ListaRelaciones = new List<TratamientoInsumo>();

        foreach (var item in tratamiento.TratamientoInsumo)
        {
            var relacion = new TratamientoInsumo(0, item.InsumoId, item.CantidadUsada);
            {
                ListaRelaciones.Add(relacion);
            }
        }


        var nuevoTratamiento = new Tratamiento(
    nuevoId,
    descripcion: tratamiento.Descripcion,
    costo: tratamiento.Costo,
    ListaRelaciones
);

        await _repositorioTratamiento.AgregarTratamiento(nuevoTratamiento);
    }

    public async Task<IEnumerable<TratamientoDtoOutput>> ObtenerTodosLosTratamientosAsync()
    {
        var tratamientos = await _repositorioTratamiento.ObtenerTodos();
        var resultado = new List<TratamientoDtoOutput>();

        foreach (var t in tratamientos)
        {
            var ListaInsumos = new List<InsumoRequeridoDtoOutput>();
            if (t.InsumosRequeridos != null)
            {
                foreach (var i in t.InsumosRequeridos)
                {
                    ListaInsumos.Add(new InsumoRequeridoDtoOutput(i.InsumoId, i.CantidadUsada));
                }

                var dto = new TratamientoDtoOutput(
                                t.Id,
                                t.Descripcion,
                                t.Costo,
                                ListaInsumos
                            );

                resultado.Add(dto);

            }


        }
        return resultado;
    }



    public async Task<TratamientoDtoOutput> ObtenerTratamientoPorIdAsync(int id)
    {
        var t = await _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (t == null) throw new KeyNotFoundException($"No se encontró el tratamiento con ID {id}");


        var listaInsumosDto = new List<InsumoRequeridoDtoOutput>();


        if (t.InsumosRequeridos != null)
        {
            foreach (var item in t.InsumosRequeridos)
            {
                var insumoDto = new InsumoRequeridoDtoOutput(item.InsumoId, item.CantidadUsada);
                listaInsumosDto.Add(insumoDto);
            }
        }


        var dto = new TratamientoDtoOutput(
            t.Id,
            t.Descripcion,
            t.Costo,
            listaInsumosDto
        );

        return dto;
    }

    public async Task<List<TratamientoDtoOutput>> BuscarTratamientosAsync(string criterio)
    {
        List<Tratamiento> tratamientos = await _repositorioTratamiento.BuscarTratamientos(criterio);
        List<TratamientoDtoOutput> resultado = new List<TratamientoDtoOutput>();

        foreach (Tratamiento t in tratamientos)
        {

            var listaInsumosDto = new List<InsumoRequeridoDtoOutput>();

            if (t.InsumosRequeridos != null)
            {
                foreach (var item in t.InsumosRequeridos)
                {
                    var insumoDto = new InsumoRequeridoDtoOutput(item.InsumoId, item.CantidadUsada);
                    listaInsumosDto.Add(insumoDto);
                }

                var dto = new TratamientoDtoOutput(
               t.Id,
               t.Descripcion,
               t.Costo,
               listaInsumosDto);

                resultado.Add(dto);

            }

        }
        return resultado;
    }



    public async Task ActualizarTratamientoAsync(int id, TratamientoDtoInput tratamientoEditado)
    {

        var existe = await _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (existe == null)
        {
            throw new KeyNotFoundException($"No se encontró el tratamiento con ID {id}.");
        }

        if (string.IsNullOrWhiteSpace(tratamientoEditado.Descripcion))
        {
            throw new ArgumentException("La descripción del tratamiento no puede estar vacía.");
        }

        if (tratamientoEditado.Costo < 0)
        {
            throw new ArgumentException("El costo no puede ser negativo.");
        }

        if (tratamientoEditado.TratamientoInsumo == null || !tratamientoEditado.TratamientoInsumo.Any())
        {
            throw new ArgumentException("Debe incluir al menos un insumo en el tratamiento.");
        }


        var listaRelaciones = new List<TratamientoInsumo>();

        foreach (var item in tratamientoEditado.TratamientoInsumo)
        {

            var relacion = new TratamientoInsumo(id, item.InsumoId, item.CantidadUsada);
            listaRelaciones.Add(relacion);
        }

        var tratamientoModificado = new Tratamiento(
            id,
            descripcion: tratamientoEditado.Descripcion,
            costo: tratamientoEditado.Costo,
            insumosRequeridos: listaRelaciones
        );

        await _repositorioTratamiento.ActualizarTratamiento(tratamientoModificado);
    }
    public async Task<bool> EliminarTratamientoAsync(int id)
    {
        var existe = await _repositorioTratamiento.ObtenerTratamientoPorId(id);
        if (existe == null) return false;

        await _repositorioTratamiento.EliminarTratamiento(id);
        return true;
    }

    public async Task<int> autoincremental()
    {
        var lista = await _repositorioTratamiento.ObtenerTodos();
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
