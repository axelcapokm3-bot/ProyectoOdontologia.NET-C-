namespace CapaDominio.Entidades;

using CapaDominio.ObjetosDeValor;
using System;
using System.Collections.Generic;

public class Turno
{
    public int Id { get; init; }
    public DateTime FechaHora => Horario.Inicio;
    public int PacienteId { get; set; }
    public int OdontologoId { get; set; }
    public int TratamientoId { get; set; }
    public EstadoTurno Estado { get; private set; }
    public IntervaloDeTiempo Horario { get; private set; }

    // Constructor de 6 parámetros
    public Turno(int id, DateTime inicio, DateTime fin, int pacienteId, int odontologoId, int tratamientoId)
    {
        if (id <= 0)
            throw new ArgumentException("El ID debe ser mayor a cero.");

        if (inicio < DateTime.Now)
            throw new ArgumentException("La fecha del turno no puede ser en el pasado.");

        if (pacienteId <= 0)
            throw new ArgumentException("El ID del paciente debe ser positivo.");

        if (odontologoId <= 0)
            throw new ArgumentException("El ID del odontólogo debe ser positivo.");

        if (tratamientoId <= 0)
            throw new ArgumentException("El ID del tratamiento debe ser positivo.");

        Id = id;
        PacienteId = pacienteId;
        OdontologoId = odontologoId;
        TratamientoId = tratamientoId;
        Estado = EstadoTurno.Pendiente;


        Horario = new IntervaloDeTiempo(inicio, fin);
    }

  public static bool EsValidoLaAgenda(IEnumerable<Turno> turnosExistentes, Turno turnoNuevo)
{
    if (turnosExistentes == null)
    {
        return true;
    }

    foreach (var turnoExistente in turnosExistentes)
    {
        if (turnoExistente.Estado != EstadoTurno.Cancelado && 
            turnoNuevo.OdontologoId == turnoExistente.OdontologoId)
        {
            if (turnoNuevo.Horario.SeSolapaCon(turnoExistente.Horario))
            {
                return false; 
            }
        }
    }

    return true;
}

    public void CambiarEstado(EstadoTurno nuevoEstado)
    {
        if (!EsValidoTransicion(Estado, nuevoEstado))
        {
            throw new InvalidOperationException($"No se puede pasar el turno de {Estado} a {nuevoEstado}.");
        }

        Estado = nuevoEstado;
    }

    public static bool EsValidoTransicion(EstadoTurno actual, EstadoTurno nuevo)
    {
        if (actual == EstadoTurno.Pendiente && (nuevo == EstadoTurno.Completado || nuevo == EstadoTurno.Cancelado))
        {
            return true;
        }
        return false; 
    }
}