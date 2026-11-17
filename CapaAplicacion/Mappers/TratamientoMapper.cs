namespace CapaAplicacion.Mappers;

using System;
using System.Collections.Generic;
using global::CapaDominio.Entidades;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;

public static class TratamientoMapper
{
    public static TratamientoDtoOutput Map(Tratamiento tratamiento)
    {
        if (tratamiento == null)
            throw new ArgumentNullException(nameof(tratamiento));

        List<InsumoRequeridoDtoOutput> insumos = new List<InsumoRequeridoDtoOutput>(
            tratamiento.InsumosRequeridos.Count);

        foreach (TratamientoInsumo insumo in tratamiento.InsumosRequeridos)
        {
            insumos.Add(new InsumoRequeridoDtoOutput(insumo.InsumoId, insumo.CantidadUsada));
        }

        return new TratamientoDtoOutput(
            tratamiento.Id,
            tratamiento.Descripcion,
            tratamiento.Costo,
            insumos,
            (int)tratamiento.DuracionEstimada.TotalMinutes);
    }

    public static List<TratamientoDtoOutput> MapMany(IReadOnlyList<Tratamiento> tratamientos)
    {
        if (tratamientos == null)
            throw new ArgumentNullException(nameof(tratamientos));

        List<TratamientoDtoOutput> resultado = new List<TratamientoDtoOutput>(tratamientos.Count);

        for (int indice = 0; indice < tratamientos.Count; indice++)
        {
            resultado.Add(Map(tratamientos[indice]));
        }

        return resultado;
    }
}
