namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
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

    public Task RegistrarInsumoAsync(InsumoDtosInput insumo)
    {
        if (!ValidadorNombre(insumo))
        {
            throw new ArgumentException("El nombre del insumo contiene caracteres no válidos o está vacío.");
        }

        int nuevoId = autoincremental();

        var nuevoInsumo = new Insumo(
            nuevoId,
            insumo.Nombre,
            insumo.Stock,
            insumo.PuntoPedido
        );

        _repositorioInsumos.AgregarInsumo(nuevoInsumo);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<InsumoDtosOutput>> ObtenerTodosLosInsumosAsync()
    {
        var insumos = _repositorioInsumos.ObtenerTodos();
        var resultado = new List<InsumoDtosOutput>();

        foreach (var i in insumos)
        {
            var dto = new InsumoDtosOutput(
                Guid.Parse(i.Id.ToString().PadLeft(32, '0')),
                i.Nombre,
                i.Stock,
                i.PuntoPedido
            );
            resultado.Add(dto);
        }

        return Task.FromResult<IEnumerable<InsumoDtosOutput>>(resultado);
    }

    public Task<InsumoDtosOutput?> ObtenerInsumoPorIdAsync(int id)
    {
        var i = _repositorioInsumos.ObtenerInsumoPorId(id);
        if (i == null) return Task.FromResult<InsumoDtosOutput?>(null);

        var dto = new InsumoDtosOutput(
            Guid.Parse(i.Id.ToString().PadLeft(32, '0')),
            i.Nombre,
            i.Stock,
            i.PuntoPedido
        );

        return Task.FromResult<InsumoDtosOutput?>(dto);
    }

    public Task ActualizarInsumoAsync(int id, InsumoDtosInput insumoEditado)
    {
        var existe = _repositorioInsumos.ObtenerInsumoPorId(id);
        if (existe == null) return Task.CompletedTask;

        var insumoModificado = new Insumo(
            id,
            insumoEditado.Nombre,
            insumoEditado.Stock,
            insumoEditado.PuntoPedido
        );

        _repositorioInsumos.ActualizarInsumo(insumoModificado);

        return Task.CompletedTask;
    }

    public Task<bool> EliminarInsumoAsync(int id)
    {
        var existe = _repositorioInsumos.ObtenerInsumoPorId(id);
        if (existe == null) return Task.FromResult(false);

        _repositorioInsumos.EliminarInsumo(id);
        return Task.FromResult(true);
    }

    // --- Métodos auxiliares ---

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

    public int autoincremental()
    {
        var lista = _repositorioInsumos.ObtenerTodos();
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