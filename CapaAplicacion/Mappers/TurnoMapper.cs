namespace CapaAplicacion.Mappers;

using System;
using System.Collections.Generic;
using global::CapaDominio.Entidades;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;

public static class TurnoMapper
{
    public static TurnoDtoOutput Map(Turno turno)
    {
        if (turno == null)
            throw new ArgumentNullException(nameof(turno));

        return new TurnoDtoOutput(
            turno.Id,
            turno.FechaHoraInicio,
            turno.FechaHoraFin,
            turno.PacienteId,
            turno.OdontologoId,
            turno.TratamientoId,
            turno.Estado);
    }

    public static List<TurnoDtoOutput> MapMany(IReadOnlyList<Turno> turnos)
    {
        if (turnos == null)
            throw new ArgumentNullException(nameof(turnos));

        List<TurnoDtoOutput> resultado = new List<TurnoDtoOutput>(turnos.Count);

        for (int indice = 0; indice < turnos.Count; indice++)
        {
            resultado.Add(Map(turnos[indice]));
        }

        return resultado;
    }
}
