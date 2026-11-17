namespace CapaAplicacion.Mappers;

using System;
using System.Collections.Generic;
using global::CapaDominio.Entidades;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;

public static class OdontologoMapper
{
    public static OdontologoOutputDto Map(Odontologo odontologo)
    {
        if (odontologo == null)
            throw new ArgumentNullException(nameof(odontologo));

        return new OdontologoOutputDto(
            odontologo.Id,
            odontologo.Nombre,
            odontologo.Matricula,
            odontologo.Especialidad,
            odontologo.Telefono);
    }

    public static List<OdontologoOutputDto> MapMany(IReadOnlyList<Odontologo> odontologos)
    {
        if (odontologos == null)
            throw new ArgumentNullException(nameof(odontologos));

        List<OdontologoOutputDto> resultado = new List<OdontologoOutputDto>(odontologos.Count);

        for (int indice = 0; indice < odontologos.Count; indice++)
        {
            resultado.Add(Map(odontologos[indice]));
        }

        return resultado;
    }
}
