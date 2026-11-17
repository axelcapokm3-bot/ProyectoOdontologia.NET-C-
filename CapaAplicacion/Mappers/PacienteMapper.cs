namespace CapaAplicacion.Mappers;

using System;
using System.Collections.Generic;
using global::CapaDominio.Entidades;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;

public static class PacienteMapper
{
    public static PacienteDtoOutput Map(Paciente paciente)
    {
        if (paciente == null)
            throw new ArgumentNullException(nameof(paciente));

        return new PacienteDtoOutput(
            paciente.Id,
            paciente.Nombre,
            paciente.Apellido,
            paciente.Email,
            paciente.FechaNacimiento,
            paciente.Telefono);
    }

    public static List<PacienteDtoOutput> MapMany(IReadOnlyList<Paciente> pacientes)
    {
        if (pacientes == null)
            throw new ArgumentNullException(nameof(pacientes));

        List<PacienteDtoOutput> resultado = new List<PacienteDtoOutput>(pacientes.Count);

        for (int indice = 0; indice < pacientes.Count; indice++)
        {
            resultado.Add(Map(pacientes[indice]));
        }

        return resultado;
    }
}
