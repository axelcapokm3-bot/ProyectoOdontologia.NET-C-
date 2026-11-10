namespace CapaDominio.ObjetosDeValor;

using System;
using System.Collections.Generic;
using CapaDominio.Entidades;
using CapaDominio.Excepciones;

public class AgendaDiaria
{
    private readonly List<Turno> _turnos = new List<Turno>();

    public DateTime Fecha { get; private set; }
    public int OdontologoId { get; private set; }

    public AgendaDiaria(DateTime fecha, int odontologoId)
    {
        Fecha = fecha.Date;
        OdontologoId = odontologoId;
    }

    public AgendaDiaria(int odontologoId, DateTime fecha, IEnumerable<Turno> turnos)
        : this(fecha, odontologoId)
    {
        if (turnos == null) throw new ArgumentNullException(nameof(turnos));

        foreach (Turno turno in turnos)
        {
            if (turno.OdontologoId == OdontologoId &&
                turno.Estado != EstadoTurno.Cancelado &&
                turno.Horario.Inicio.Date == Fecha)
            {
                _turnos.Add(turno);
            }
        }

        _turnos.Sort(CompararPorInicio);
    }

    public IReadOnlyCollection<Turno> Turnos => _turnos.AsReadOnly();

    public void AgregarTurno(Turno turno)
    {
        if (turno == null) throw new ArgumentNullException(nameof(turno));

        if (turno.OdontologoId != OdontologoId)
            throw new ArgumentException("El turno no corresponde a este odontólogo.");

        ValidarDisponibilidad(turno);

        _turnos.Add(turno);
        _turnos.Sort(CompararPorInicio);
    }

    public void ValidarDisponibilidad(Turno turnoNuevo)
    {
        if (turnoNuevo == null) throw new ArgumentNullException(nameof(turnoNuevo));

        ValidarDisponibilidad(_turnos, turnoNuevo);
    }

    public static void ValidarDisponibilidad(IReadOnlyList<Turno> turnosOrdenados, Turno turnoNuevo)
    {
        if (turnosOrdenados == null) throw new ArgumentNullException(nameof(turnosOrdenados));
        if (turnoNuevo == null) throw new ArgumentNullException(nameof(turnoNuevo));

        DateTime inicioNuevo = turnoNuevo.FechaHoraInicio;
        DateTime finNuevo = turnoNuevo.FechaHoraFin;

        if (finNuevo <= inicioNuevo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(turnoNuevo),
                "La fecha de fin del turno debe ser posterior a la fecha de inicio.");
        }

        DateTime finTurnoAnterior = DateTime.MinValue;
        bool hayTurnoAnterior = false;
        bool candidatoValidado = false;

        for (int i = 0; i < turnosOrdenados.Count; i++)
        {
            Turno turnoActual = turnosOrdenados[i];

            if (turnoActual == null)
            {
                throw new ArgumentException(
                    "La lista de turnos contiene un elemento nulo.",
                    nameof(turnosOrdenados));
            }

            if (turnoActual.Estado == EstadoTurno.Cancelado)
            {
                continue;
            }

            if (turnoActual.OdontologoId != turnoNuevo.OdontologoId)
            {
                continue;
            }

            if (turnoActual.FechaHoraInicio.Date != inicioNuevo.Date)
            {
                continue;
            }

            if (hayTurnoAnterior && finTurnoAnterior > turnoActual.FechaHoraInicio)
            {
                throw new TurnoSolapadoException(
                    "La agenda contiene turnos solapados en el mismo rango horario.");
            }

            if (!candidatoValidado)
            {
                if (finNuevo <= turnoActual.FechaHoraInicio)
                {
                    if (hayTurnoAnterior && inicioNuevo < finTurnoAnterior)
                    {
                        throw new TurnoSolapadoException(
                            "El hueco disponible entre turnos es menor a la duración requerida.");
                    }

                    candidatoValidado = true;
                }
                else if (inicioNuevo < turnoActual.FechaHoraFin)
                {
                    throw new TurnoSolapadoException(
                        "El turno se solapa con otro turno existente de la agenda.");
                }
            }

            finTurnoAnterior = turnoActual.FechaHoraFin;
            hayTurnoAnterior = true;
        }

        if (!candidatoValidado && hayTurnoAnterior && inicioNuevo < finTurnoAnterior)
        {
            throw new TurnoSolapadoException(
                "El turno se solapa con otro turno existente de la agenda.");
        }
    }

    public bool TieneSolapamiento(Turno turnoNuevo)
    {
        if (turnoNuevo == null) throw new ArgumentNullException(nameof(turnoNuevo));

        try
        {
            ValidarDisponibilidad(turnoNuevo);
            return false;
        }
        catch (TurnoSolapadoException)
        {
            return true;
        }
    }

    public bool TieneSolapamiento()
    {
        List<Turno> activos = ObtenerActivosOrdenados();
        Turno? turnoAnterior = null;

        for (int i = 0; i < activos.Count; i++)
        {
            Turno turnoActual = activos[i];

            if (turnoAnterior != null && turnoAnterior.FechaHoraFin > turnoActual.FechaHoraInicio)
            {
                return true;
            }

            turnoAnterior = turnoActual;
        }

        return false;
    }

    public IEnumerable<IntervaloDeTiempo> ObtenerHuecosDisponibles(
        TimeSpan inicioJornada,
        TimeSpan finJornada,
        TimeSpan duracionMinima)
    {
        List<Turno> turnosOrdenados = ObtenerActivosOrdenados();
        List<IntervaloDeTiempo> huecos = new List<IntervaloDeTiempo>();

        DateTime puntero = Fecha.Add(inicioJornada);

        if (Fecha.Date == DateTime.Today && puntero < DateTime.Now)
        {
            puntero = DateTime.Now;
        }

        for (int i = 0; i < turnosOrdenados.Count; i++)
        {
            Turno turno = turnosOrdenados[i];

            if (turno.Horario.Inicio > puntero && (turno.Horario.Inicio - puntero) >= duracionMinima)
            {
                huecos.Add(new IntervaloDeTiempo(puntero, turno.Horario.Inicio));
            }

            if (turno.Horario.Fin > puntero)
            {
                puntero = turno.Horario.Fin;
            }
        }

        DateTime finDelDia = Fecha.Add(finJornada);

        if (puntero < finDelDia && (finDelDia - puntero) >= duracionMinima)
        {
            huecos.Add(new IntervaloDeTiempo(puntero, finDelDia));
        }

        return huecos;
    }

    private List<Turno> ObtenerActivosOrdenados()
    {
        List<Turno> activos = new List<Turno>();

        foreach (Turno turno in _turnos)
        {
            if (turno.Estado != EstadoTurno.Cancelado)
            {
                activos.Add(turno);
            }
        }

        activos.Sort(CompararPorInicio);
        return activos;
    }

    private static int CompararPorInicio(Turno primero, Turno segundo)
    {
        return primero.FechaHoraInicio.CompareTo(segundo.FechaHoraInicio);
    }
}
