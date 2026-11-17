namespace CapaAplicacion.Mappers;

using System;
using System.Collections.Generic;
using global::CapaDominio.Entidades;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;

public static class InsumoMapper
{
    public static InsumoDtosOutput Map(Insumo insumo)
    {
        if (insumo == null)
            throw new ArgumentNullException(nameof(insumo));

        return new InsumoDtosOutput(
            insumo.Id,
            insumo.Nombre,
            insumo.Stock,
            insumo.StockReservado,
            insumo.PuntoPedido,
            insumo.StockMinimo);
    }

    public static List<InsumoDtosOutput> MapMany(IReadOnlyList<Insumo> insumos)
    {
        if (insumos == null)
            throw new ArgumentNullException(nameof(insumos));

        List<InsumoDtosOutput> resultado = new List<InsumoDtosOutput>(insumos.Count);

        for (int indice = 0; indice < insumos.Count; indice++)
        {
            resultado.Add(Map(insumos[indice]));
        }

        return resultado;
    }
}
