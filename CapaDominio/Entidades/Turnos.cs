namespace CapaDominio.Entidades;

using CapaDominio.ObjetosDeValor;
using System;
using System.Collections.Generic;

public class Turno
{
    public int Id { get; private set; }
    public DateTime FechaHoraInicio { get; private set; }
    public DateTime FechaHoraFin { get; private set; }
    public DateTime FechaHora => FechaHoraInicio;
    public int PacienteId { get; private set; }
    public int OdontologoId { get; private set; }
    public int TratamientoId { get; private set; }
    public EstadoTurno Estado { get; private set; }
    public IntervaloDeTiempo Horario => new IntervaloDeTiempo(FechaHoraInicio, FechaHoraFin);

    private Turno()
    {
    }

    public Turno(int id, DateTime fechaHoraInicio, DateTime fechaHoraFin, int pacienteId, int odontologoId, int tratamientoId)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));
        if (pacienteId <= 0) throw new ArgumentException("El ID del paciente debe ser positivo.", nameof(pacienteId));
        if (odontologoId <= 0) throw new ArgumentException("El ID del odontólogo debe ser positivo.", nameof(odontologoId));
        if (tratamientoId <= 0) throw new ArgumentException("El ID del tratamiento debe ser positivo.", nameof(tratamientoId));
        if (fechaHoraFin <= fechaHoraInicio) throw new ArgumentOutOfRangeException(nameof(fechaHoraFin), "La fecha de fin debe ser posterior a la de inicio.");

        Id = id;
        PacienteId = pacienteId;
        OdontologoId = odontologoId;
        TratamientoId = tratamientoId;
        Estado = EstadoTurno.Pendiente;
        FechaHoraInicio = fechaHoraInicio;
        FechaHoraFin = fechaHoraFin;
    }

    public Turno(int id, DateTime fechaHoraInicio, DateTime fechaHoraFin, int pacienteId, int odontologoId, int tratamientoId, EstadoTurno estado)
        : this(id, fechaHoraInicio, fechaHoraFin, pacienteId, odontologoId, tratamientoId)
    {
        if (!Enum.IsDefined(estado))
            throw new ArgumentOutOfRangeException(nameof(estado), "El estado del turno no es válido.");

        Estado = estado;
    }

    public void Cancelar()
    {
        if (Estado == EstadoTurno.Cancelado) return;
        if (Estado == EstadoTurno.Completado) throw new InvalidOperationException("No se puede cancelar un turno completado.");
        Estado = EstadoTurno.Cancelado;
    }

    public void Completar()
    {
        if (Estado == EstadoTurno.Completado) return;
        if (Estado == EstadoTurno.Cancelado) throw new InvalidOperationException("No se puede completar un turno cancelado.");
        Estado = EstadoTurno.Completado;
    }

    public void Reprogramar(DateTime inicio, DateTime fin)
    {
        if (Estado == EstadoTurno.Cancelado || Estado == EstadoTurno.Completado)
            throw new InvalidOperationException("No se puede reprogramar un turno finalizado.");
        if (fin <= inicio) throw new ArgumentOutOfRangeException(nameof(fin), "La fecha de fin debe ser posterior a la de inicio.");

        FechaHoraInicio = inicio;
        FechaHoraFin = fin;
    }
}