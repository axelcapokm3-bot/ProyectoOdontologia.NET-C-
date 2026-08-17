namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class InsumosService : IInsumosService
{
    private readonly IRepositorioInsumos _repositorioInsumos;

    public InsumosService(IRepositorioInsumos repositorioInsumos)
    {
        _repositorioInsumos = repositorioInsumos;
    }

    public async Task RegistrarInsumoAsync(InsumoDtosInput insumo)
    {
        if (!ValidadorNombre(insumo))
        {
            throw new ArgumentException("El nombre del insumo contiene caracteres no válidos o está vacío.");
        }

        int nuevoId = await autoincremental();

        var nuevoInsumo = new Insumo(
            nuevoId,
            insumo.Nombre,
            insumo.Categoria,
            insumo.Stock
        );

        await _repositorioInsumos.AgregarInsumo(nuevoInsumo);
    }

    public async Task<IEnumerable<InsumoDtosOutput>> ObtenerTodosLosInsumosAsync()
    {
        var insumos = await _repositorioInsumos.ObtenerTodos();
        var resultado = new List<InsumoDtosOutput>();

        foreach (var i in insumos)
        {
            var dto = new InsumoDtosOutput(
                i.Id,
                i.Nombre,
                i.Stock,
                i.StockReservado,
                i.PuntoPedido
            );
            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task<InsumoDtosOutput?> ObtenerInsumoPorIdAsync(int id)
    {
        var i = await _repositorioInsumos.ObtenerInsumoPorId(id);
        if (i == null) return null;

        var dto = new InsumoDtosOutput(
            i.Id,
            i.Nombre,
            i.Stock,
            i.StockReservado,
            i.PuntoPedido
        );

        return dto;
    }

    public async Task<List<InsumoDtosOutput>> BuscarInsumosAsync(string criterio)
    {
        List<Insumo> insumos = await _repositorioInsumos.BuscarInsumos(criterio);
        List<InsumoDtosOutput> resultado = new List<InsumoDtosOutput>();

        foreach (Insumo i in insumos)
        {
            var dto = new InsumoDtosOutput(
                i.Id,
                i.Nombre,
                i.Stock,
                i.StockReservado,
                i.PuntoPedido
            );
            resultado.Add(dto);
        }

        return resultado;
    }

    public async Task ActualizarInsumoAsync(int id, InsumoDtosInput insumoEditado)
    {
        var existe = await _repositorioInsumos.ObtenerInsumoPorId(id);
        if (existe == null) return;

        var insumoModificado = new Insumo(
            id,
            insumoEditado.Nombre,
            insumoEditado.Categoria,
            insumoEditado.Stock
        );

        await _repositorioInsumos.ActualizarInsumo(insumoModificado);
    }

    public async Task<bool> EliminarInsumoAsync(int id)
    {
        var existe = await _repositorioInsumos.ObtenerInsumoPorId(id);
        if (existe == null) return false;

        await _repositorioInsumos.EliminarInsumo(id);
        return true;
    }

    public bool ValidadorNombre(InsumoDtosInput insumo)
    {
        if (string.IsNullOrWhiteSpace(insumo.Nombre)) return false;

        foreach (char c in insumo.Nombre)
        {
            if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
            {
                return false;
            }
        }
        return true;
    }

    public async Task<int> autoincremental()
    {
        var lista = await _repositorioInsumos.ObtenerTodos();
        int idMax = 0;

        foreach (var i in lista)
        {
            if (i.Id > idMax)
            {
                idMax = i.Id;
            }
        }

        return idMax + 1;
    }
}
