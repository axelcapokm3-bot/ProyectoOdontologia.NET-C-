namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using CapaDominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TratamientoService : ITratamientoService
{
    private readonly IRepositorioTratamiento _repositorioTratamiento;
    private readonly IRepositorioInsumos _repositorioInsumos;
    private readonly IUnitOfWork _unitOfWork;

    public TratamientoService(
        IRepositorioTratamiento repositorioTratamiento,
        IRepositorioInsumos repositorioInsumos,
        IUnitOfWork unitOfWork)
    {
        _repositorioTratamiento = repositorioTratamiento;
        _repositorioInsumos = repositorioInsumos;
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarTratamientoAsync(TratamientoDtoInput tratamiento)
    {
        ValidarDatosTratamiento(tratamiento);

        await _unitOfWork.BeginTransactionAsync();

        int idTratamiento = _repositorioTratamiento.IdAutoIncrementoSincrono();

        List<TratamientoInsumo> relaciones = await CrearRelacionesAsync(
            idTratamiento,
            tratamiento.TratamientoInsumo);

        Tratamiento nuevoTratamiento = new Tratamiento(
            idTratamiento,
            tratamiento.Descripcion,
            tratamiento.Costo,
            relaciones,
            TimeSpan.FromMinutes(tratamiento.DuracionMinutos));

        await _repositorioTratamiento.AgregarTratamiento(nuevoTratamiento);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<TratamientoDtoOutput>> ObtenerTodosLosTratamientosAsync()
    {
        IEnumerable<Tratamiento> tratamientos = await _repositorioTratamiento.ObtenerTodos();
        List<TratamientoDtoOutput> resultado = new List<TratamientoDtoOutput>();

        foreach (Tratamiento tratamiento in tratamientos)
        {
            resultado.Add(CrearDto(tratamiento));
        }

        return resultado;
    }

    public async Task<TratamientoDtoOutput> ObtenerTratamientoPorIdAsync(int id)
    {
        Tratamiento? tratamiento = await _repositorioTratamiento.ObtenerTratamientoPorId(id);

        if (tratamiento == null)
        {
            throw new TratamientoNoEncontradoException(id);
        }

        return CrearDto(tratamiento);
    }

    public async Task<List<TratamientoDtoOutput>> BuscarTratamientosAsync(string criterio)
    {
        List<Tratamiento> tratamientos = await _repositorioTratamiento.BuscarTratamientos(criterio);
        List<TratamientoDtoOutput> resultado = new List<TratamientoDtoOutput>();

        foreach (Tratamiento tratamiento in tratamientos)
        {
            resultado.Add(CrearDto(tratamiento));
        }

        return resultado;
    }

    public async Task ActualizarTratamientoAsync(int id, TratamientoDtoInput tratamientoEditado)
    {
        ValidarDatosTratamiento(tratamientoEditado);

        await _unitOfWork.BeginTransactionAsync();

        Tratamiento? existente = await _repositorioTratamiento.ObtenerTratamientoPorId(id);

        if (existente == null)
        {
            throw new TratamientoNoEncontradoException(id);
        }

        List<TratamientoInsumo> relaciones = await CrearRelacionesAsync(id, tratamientoEditado.TratamientoInsumo);

        Tratamiento tratamientoModificado = new Tratamiento(
            id,
            tratamientoEditado.Descripcion,
            tratamientoEditado.Costo,
            relaciones,
            TimeSpan.FromMinutes(tratamientoEditado.DuracionMinutos));

        await _repositorioTratamiento.ActualizarTratamiento(tratamientoModificado);
        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> EliminarTratamientoAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();

        Tratamiento? existente = await _repositorioTratamiento.ObtenerTratamientoPorId(id);

        if (existente == null)
        {
            throw new TratamientoNoEncontradoException(id);
        }

        bool eliminado = await _repositorioTratamiento.EliminarTratamiento(id);

        if (!eliminado)
        {
            throw new TratamientoNoEncontradoException(id);
        }

        await _unitOfWork.CommitAsync();

        return true;
    }

    private static void ValidarDatosTratamiento(TratamientoDtoInput tratamiento)
    {
        if (tratamiento == null)
        {
            throw new ArgumentNullException(nameof(tratamiento));
        }

        if (string.IsNullOrWhiteSpace(tratamiento.Descripcion))
        {
            throw new ArgumentException("La descripción del tratamiento no puede estar vacía.");
        }

        if (tratamiento.Costo < 0)
        {
            throw new ArgumentException("El costo no puede ser negativo.");
        }

        if (tratamiento.DuracionMinutos <= 0)
        {
            throw new ArgumentException("La duración del tratamiento debe ser mayor a cero.");
        }

        if (tratamiento.TratamientoInsumo == null)
        {
            throw new ArgumentException("Debe incluir al menos un insumo en el tratamiento.");
        }

        int cantidadInsumos = 0;

        foreach (InsumoRequeridoDtoInput item in tratamiento.TratamientoInsumo)
        {
            cantidadInsumos++;
        }

        if (cantidadInsumos == 0)
        {
            throw new ArgumentException("Debe incluir al menos un insumo en el tratamiento.");
        }
    }

    private async Task<List<TratamientoInsumo>> CrearRelacionesAsync(
        int tratamientoId,
        List<InsumoRequeridoDtoInput> insumosRequeridos)
    {
        List<TratamientoInsumo> relaciones = new List<TratamientoInsumo>();

        foreach (InsumoRequeridoDtoInput item in insumosRequeridos)
        {
            if (item.InsumoId <= 0)
            {
                throw new ArgumentException("El ID del insumo debe ser mayor a cero.");
            }

            if (item.CantidadUsada <= 0)
            {
                throw new ArgumentException("La cantidad usada debe ser mayor a cero.");
            }

            Insumo? insumo = await _repositorioInsumos.ObtenerInsumoPorId(item.InsumoId);

            if (insumo == null)
            {
                throw new InsumoNoEncontradoException(item.InsumoId);
            }

            relaciones.Add(new TratamientoInsumo(tratamientoId, item.InsumoId, item.CantidadUsada));
        }

        return relaciones;
    }

    private static TratamientoDtoOutput CrearDto(Tratamiento tratamiento)
    {
        List<InsumoRequeridoDtoOutput> listaInsumos = new List<InsumoRequeridoDtoOutput>();

        foreach (TratamientoInsumo insumo in tratamiento.InsumosRequeridos)
        {
            listaInsumos.Add(new InsumoRequeridoDtoOutput(insumo.InsumoId, insumo.CantidadUsada));
        }

        return new TratamientoDtoOutput(
            tratamiento.Id,
            tratamiento.Descripcion,
            tratamiento.Costo,
            listaInsumos,
            (int)tratamiento.DuracionEstimada.TotalMinutes);
    }
}
