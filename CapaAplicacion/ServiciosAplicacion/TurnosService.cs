namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using global::CapaDominio.ObjetosDeValor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TurnosService : ITurnosService
{
    private readonly IRepositorioTurnos _repositorioTurnos;

    public TurnosService(IRepositorioTurnos repositorioTurnos)
    {
        _repositorioTurnos = repositorioTurnos;
    }

    public async Task RegistrarTurnoAsync(TurnoDtoInput turnoDto)
    {
        if (turnoDto.FechaHoraInicio < DateTime.Now)
        {
            throw new ArgumentException("La fecha y hora del turno no puede ser en el pasado.");
        }

        int nuevoId = await autoincremental();

        var nuevoTurno = new Turno(
            nuevoId,
            turnoDto.FechaHoraInicio,
            turnoDto.FechaHoraFin,
            turnoDto.PacienteId,
            turnoDto.OdontologoId,
            turnoDto.TratamientoId
        );

        var turnosExistentes = await _repositorioTurnos.ObtenerTodos();

        if (!Turno.EsValidoLaAgenda(turnosExistentes, nuevoTurno))
        {
            throw new InvalidOperationException("El odontólogo ya posee un turno asignado que se solapa en ese horario.");
        }

        await _repositorioTurnos.AgregarTurno(nuevoTurno);
    }

    public async Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id)
    {
        var t = await _repositorioTurnos.ObtenerTurnoPorId(id);
        if (t is null) 
            throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

        return new TurnoDtoOutput(
            t.Id,
            t.Horario.Inicio,
            t.PacienteId,
            t.OdontologoId,
            t.TratamientoId
        );
    }

    public async Task<List<TurnoDtoOutput>> BuscarTurnosAsync(string criterio)
    {
        List<Turno> turnos = await _repositorioTurnos.BuscarTurnos(criterio);
        List<TurnoDtoOutput> resultado = new List<TurnoDtoOutput>();

        foreach (Turno t in turnos)
        {
            resultado.Add(new TurnoDtoOutput(t.Id, t.Horario.Inicio, t.PacienteId, t.OdontologoId, t.TratamientoId));
        }

        return resultado;
    }

    public async Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado)
    {
        var existe = await _repositorioTurnos.ObtenerTurnoPorId(id);
        if (existe == null) 
            throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

        var turnoModificado = new Turno(
            id,
            turnoEditado.FechaHoraInicio,
            turnoEditado.FechaHoraFin,
            turnoEditado.PacienteId,
            turnoEditado.OdontologoId,
            turnoEditado.TratamientoId
        );

        await _repositorioTurnos.ActualizarTurno(turnoModificado);
    }

    public async Task<bool> EliminarTurnoAsync(int id)
    {
        var existe = await _repositorioTurnos.ObtenerTurnoPorId(id);
        if (existe == null) return false;

        await _repositorioTurnos.EliminarTurno(id);
        return true;
    }
    public async Task<IEnumerable<TurnoDtoOutput>> ObtenerTodosLosTurnosAsync()
{
    var turnos = await _repositorioTurnos.ObtenerTodos();
    var resultado = new List<TurnoDtoOutput>();

    foreach (var t in turnos)
    {
        resultado.Add(new TurnoDtoOutput(t.Id, t.Horario.Inicio, t.PacienteId, t.OdontologoId, t.TratamientoId));
    }

    return resultado;
}

    public async Task<List<IntervaloDeTiempo>> ObtenerHuecosDisponiblesAsync(int odontologoId, DateTime fecha, TimeSpan duracionRequerida)
    {
        var todos = await _repositorioTurnos.ObtenerTodos();
        List<Turno> turnosDelDia = new List<Turno>();

        foreach (var turno in todos)
        {
            if (turno.OdontologoId == odontologoId &&
                turno.Estado != EstadoTurno.Cancelado &&
                turno.Horario.Inicio.Date == fecha.Date)
            {
                turnosDelDia.Add(turno);
            }
        }

        turnosDelDia.Sort((a, b) => a.Horario.Inicio.CompareTo(b.Horario.Inicio));

        List<IntervaloDeTiempo> huecosEncontrados = new List<IntervaloDeTiempo>();

        DateTime inicioJornada = fecha.Date.AddHours(8);
        DateTime finJornada = fecha.Date.AddHours(18);
        DateTime puntero = inicioJornada;

        foreach (var t in turnosDelDia)
        {
            if (puntero < t.Horario.Inicio)
            {
                var hueco = new IntervaloDeTiempo(puntero, t.Horario.Inicio);
                if (hueco.Duracion >= duracionRequerida)
                {
                    huecosEncontrados.Add(hueco);
                }
            }

            if (t.Horario.Fin > puntero)
            {
                puntero = t.Horario.Fin;
            }
        }

        if (puntero < finJornada)
        {
            var huecoFinal = new IntervaloDeTiempo(puntero, finJornada);
            if (huecoFinal.Duracion >= duracionRequerida)
            {
                huecosEncontrados.Add(huecoFinal);
            }
        }

        return huecosEncontrados;
    }

    public async Task<int> autoincremental()
    {
        var lista = await _repositorioTurnos.ObtenerTodos();
        int idMax = 0;

        foreach (var t in lista)
        {
            if (t.Id > idMax)
            {
                idMax = t.Id;
            }
        }

        return idMax + 1;
    }
}