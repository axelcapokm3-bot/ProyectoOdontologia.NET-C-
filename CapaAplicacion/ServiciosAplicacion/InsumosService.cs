namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::CapaAplicacion.Mappers;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using CapaDominio.Entidades;
using CapaDominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InsumosService : IInsumosService
{
    private readonly IRepositorioInsumos _repositorioInsumos;
    private readonly IUnitOfWork _unitOfWork;

    public InsumosService(IRepositorioInsumos repositorioInsumos, IUnitOfWork unitOfWork)
    {
        _repositorioInsumos = repositorioInsumos;
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarInsumoAsync(InsumoDtosInput insumo)
    {
        if (!ValidadorNombre(insumo))
        {
            throw new ArgumentException("El nombre del insumo contiene caracteres no válidos o está vacío.");
        }

        await _unitOfWork.BeginTransactionAsync();

        int idInsumo = _repositorioInsumos.IdAutoIncrementoSincrono();

        Insumo nuevoInsumo = new Insumo(
            idInsumo,
            insumo.Nombre,
            insumo.Categoria,
            insumo.Stock);

        await _repositorioInsumos.AgregarInsumo(nuevoInsumo);
        await _unitOfWork.CommitAsync();
    }

    public async Task<IEnumerable<InsumoDtosOutput>> ObtenerTodosLosInsumosAsync()
    {
        IEnumerable<Insumo> insumos = await _repositorioInsumos.ObtenerTodos();
        List<Insumo> listaInsumos = new List<Insumo>();

        foreach (Insumo insumo in insumos)
        {
            listaInsumos.Add(insumo);
        }

        return InsumoMapper.MapMany(listaInsumos);
    }

    public async Task<InsumoDtosOutput> ObtenerInsumoPorIdAsync(int id)
    {
        Insumo? insumo = await _repositorioInsumos.ObtenerInsumoPorId(id);

        if (insumo == null)
        {
            throw new InsumoNoEncontradoException(id);
        }

        return InsumoMapper.Map(insumo);
    }

    public async Task<List<InsumoDtosOutput>> BuscarInsumosAsync(string criterio)
    {
        List<Insumo> insumos = await _repositorioInsumos.BuscarInsumos(criterio);
        return InsumoMapper.MapMany(insumos);
    }

    public async Task ActualizarInsumoAsync(int id, InsumoDtosInput insumoEditado)
    {
        if (!ValidadorNombre(insumoEditado))
        {
            throw new ArgumentException("El nombre del insumo contiene caracteres no válidos o está vacío.");
        }

        await _unitOfWork.BeginTransactionAsync();

        Insumo? insumo = await _repositorioInsumos.ObtenerInsumoPorId(id);

        if (insumo == null)
        {
            throw new InsumoNoEncontradoException(id);
        }

        insumo.Actualizar(
            insumoEditado.Nombre,
            insumoEditado.Categoria,
            insumoEditado.Stock,
            insumo.StockReservado);

        await _repositorioInsumos.ActualizarInsumo(insumo);
        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> EliminarInsumoAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();

        Insumo? insumo = await _repositorioInsumos.ObtenerInsumoPorId(id);

        if (insumo == null)
        {
            throw new InsumoNoEncontradoException(id);
        }

        bool eliminado = await _repositorioInsumos.EliminarInsumo(id);

        if (!eliminado)
        {
            throw new InsumoNoEncontradoException(id);
        }

        await _unitOfWork.CommitAsync();

        return true;
    }

    public bool ValidadorNombre(InsumoDtosInput insumo)
    {
        if (string.IsNullOrWhiteSpace(insumo.Nombre))
        {
            return false;
        }

        foreach (char caracter in insumo.Nombre)
        {
            if (!char.IsLetter(caracter) && !char.IsWhiteSpace(caracter))
            {
                return false;
            }
        }

        return true;
    }

}
