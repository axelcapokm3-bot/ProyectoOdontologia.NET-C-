namespace CapaInfraestructura.Implementacion;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;

public class RepositorioInsumos : IRepositorioInsumos
{
    private static List<Insumo> _insumos = new List<Insumo>();

    public RepositorioInsumos()
    {
        _insumos = new List<Insumo>();
    }

    public List<Insumo> ObtenerTodos()
    {
        return _insumos;
    }

    public Insumo? ObtenerInsumoPorId(int id)
    {
        foreach (var i in _insumos)
        {
            if (i.Id == id)
            {
                return i;
            }
        }
        return null;
    }

    public void AgregarInsumo(Insumo insumo)
    {
        _insumos.Add(insumo);
    }

    public void ActualizarInsumo(Insumo insumo)
    {
        if (insumo == null)
        {

        }

        foreach (Insumo i in _insumos)
        {
            if (i.Id == insumo.Id)
            {
                i.Nombre = insumo.Nombre;
                i.Stock = insumo.Stock;
                i.PuntoPedido = insumo.PuntoPedido;
            }
        }

        return;
    }

    public void EliminarInsumo(int id)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");
        }

        for (int i = 0; i < _insumos.Count; i++)
        {
            if (_insumos[i].Id == id)
            {
                _insumos.RemoveAt(i);
            }
        }

        return;
    }
}
