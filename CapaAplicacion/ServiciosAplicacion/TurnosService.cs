namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using global::CapaDominio.Excepciones;
using global::CapaDominio.ObjetosDeValor;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class TurnosService : ITurnosService
{
    private readonly IRepositorioTurnos _repositorioTurnos;
    private readonly IRepositorioTratamiento _repositorioTratamiento;
    private readonly IRepositorioPaciente _repositorioPaciente;
    private readonly IUnitOfWork _unitOfWork;

    public TurnosService(
        IRepositorioTurnos repositorioTurnos,
        IRepositorioTratamiento repositorioTratamiento,
        IRepositorioPaciente repositorioPaciente,
        IUnitOfWork unitOfWork)
    {
        _repositorioTurnos = repositorioTurnos;
        _repositorioTratamiento = repositorioTratamiento;
        _repositorioPaciente = repositorioPaciente;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> RegistrarTurnoAsync(CrearTurnoRequestDto nuevoTurno, CancellationToken cancellationToken = default)
    {
        if (nuevoTurno == null)
            throw new ArgumentNullException(nameof(nuevoTurno));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        Tratamiento? tratamiento = await _repositorioTratamiento.ObtenerTratamientoPorIdAsync(
            nuevoTurno.TratamientoId,
            cancellationToken);

        if (tratamiento == null)
            throw new TratamientoNoEncontradoException(nuevoTurno.TratamientoId);

        Paciente? paciente = await _repositorioPaciente.ObtenerPacientePorId(nuevoTurno.PacienteId);

        if (paciente == null)
            throw new PacienteNoEncontradoException(nuevoTurno.PacienteId);

        DateTime inicio = nuevoTurno.FechaHoraInicio;
        DateTime fin = inicio.Add(tratamiento.DuracionEstimada);

        IReadOnlyList<Turno> turnosDelDia = await _repositorioTurnos.ObtenerTurnosDelDiaAsync(
            nuevoTurno.OdontologoId,
            inicio.Date,
            cancellationToken);

        int nuevoId = await _repositorioTurnos.ObtenerProximoIdAsync(cancellationToken);
        Turno turno = new Turno(
            nuevoId,
            inicio,
            fin,
            nuevoTurno.PacienteId,
            nuevoTurno.OdontologoId,
            nuevoTurno.TratamientoId);

        AgendaDiaria.ValidarDisponibilidad(turnosDelDia, turno);

        await _repositorioTurnos.AgregarTurnoAsync(turno, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return turno.Id;
    }

    public async Task<List<IntervaloTiempoDto>> ConsultarHuecosDisponiblesAsync(
        ConsultarHuecosRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.DuracionMinutos <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "La duración mínima debe ser mayor a 0 minutos.");

        IReadOnlyList<Turno> turnosDelDia = await _repositorioTurnos.ObtenerTurnosDelDiaAsync(
            request.OdontologoId,
            request.Fecha.Date,
            cancellationToken);

        AgendaDiaria agenda = new AgendaDiaria(request.OdontologoId, request.Fecha.Date, turnosDelDia);

        IEnumerable<IntervaloDeTiempo> huecos = agenda.ObtenerHuecosDisponibles(
            TimeSpan.FromHours(8),
            TimeSpan.FromHours(18),
            TimeSpan.FromMinutes(request.DuracionMinutos));

        List<IntervaloTiempoDto> resultado = new List<IntervaloTiempoDto>();

        foreach (IntervaloDeTiempo hueco in huecos)
        {
            resultado.Add(new IntervaloTiempoDto(hueco.Inicio, hueco.Fin));
        }

        return resultado;
    }

    public async Task CancelarTurnoAsync(int id, CancellationToken cancellationToken = default)
    {
        Turno? turno = await _repositorioTurnos.ObtenerTurnoPorIdAsync(id, cancellationToken);

        if (turno == null)
            throw new TurnoNoEncontradoException(id);

        turno.Cancelar();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _repositorioTurnos.ActualizarTurnoAsync(turno, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task RegistrarTurnoAsync(TurnoDtoInput turnoDto)
    {
        if (turnoDto == null)
            throw new ArgumentNullException(nameof(turnoDto));

        await _unitOfWork.BeginTransactionAsync();

        IEnumerable<Turno> turnosOdontologo = await _repositorioTurnos.ObtenerTodos();

        int nuevoId = _repositorioTurnos.IdAutoIncrementoSincrono();

        Turno nuevoTurno = new Turno(
            nuevoId,
            turnoDto.FechaHoraInicio,
            turnoDto.FechaHoraFin,
            turnoDto.PacienteId,
            turnoDto.OdontologoId,
            turnoDto.TratamientoId);

        AgendaDiaria agendaDelDia = new AgendaDiaria(
            nuevoTurno.OdontologoId,
            nuevoTurno.FechaHoraInicio.Date,
            new List<Turno>());

        foreach (Turno turnoExistente in turnosOdontologo)
        {
            if (turnoExistente.OdontologoId == nuevoTurno.OdontologoId &&
                turnoExistente.FechaHoraInicio.Date == nuevoTurno.FechaHoraInicio.Date &&
                turnoExistente.Estado != EstadoTurno.Cancelado)
            {
                agendaDelDia.AgregarTurno(turnoExistente);
            }
        }

        agendaDelDia.ValidarDisponibilidad(nuevoTurno);

        await _repositorioTurnos.AgregarTurno(nuevoTurno);
        await _unitOfWork.CommitAsync();
    }

    public async Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id)
    {
        Turno? turno = await _repositorioTurnos.ObtenerTurnoPorId(id);

        if (turno == null)
            throw new TurnoNoEncontradoException(id);

        return new TurnoDtoOutput(
            turno.Id,
            turno.Horario.Inicio,
            turno.PacienteId,
            turno.OdontologoId,
            turno.TratamientoId);
    }

    public async Task<List<TurnoDtoOutput>> BuscarTurnosAsync(string criterio)
    {
        List<Turno> turnos = await _repositorioTurnos.BuscarTurnos(criterio);
        List<TurnoDtoOutput> resultado = new List<TurnoDtoOutput>();

        foreach (Turno turno in turnos)
        {
            resultado.Add(new TurnoDtoOutput(
                turno.Id,
                turno.Horario.Inicio,
                turno.PacienteId,
                turno.OdontologoId,
                turno.TratamientoId));
        }

        return resultado;
    }

    public async Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado)
    {
        if (turnoEditado == null)
            throw new ArgumentNullException(nameof(turnoEditado));

        await _unitOfWork.BeginTransactionAsync();

        Turno? existe = await _repositorioTurnos.ObtenerTurnoPorId(id);

        if (existe == null)
            throw new TurnoNoEncontradoException(id);

        Turno turnoModificado = new Turno(
            id,
            turnoEditado.FechaHoraInicio,
            turnoEditado.FechaHoraFin,
            turnoEditado.PacienteId,
            turnoEditado.OdontologoId,
            turnoEditado.TratamientoId);

        await _repositorioTurnos.ActualizarTurno(turnoModificado);
        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> EliminarTurnoAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();

        Turno? existe = await _repositorioTurnos.ObtenerTurnoPorId(id);

        if (existe == null)
            throw new TurnoNoEncontradoException(id);

        bool eliminado = await _repositorioTurnos.EliminarTurno(id);

        if (!eliminado)
            throw new TurnoNoEncontradoException(id);

        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<IEnumerable<TurnoDtoOutput>> ObtenerTodosLosTurnosAsync()
    {
        IEnumerable<Turno> turnos = await _repositorioTurnos.ObtenerTodos();
        List<TurnoDtoOutput> resultado = new List<TurnoDtoOutput>();

        foreach (Turno turno in turnos)
        {
            resultado.Add(new TurnoDtoOutput(
                turno.Id,
                turno.Horario.Inicio,
                turno.PacienteId,
                turno.OdontologoId,
                turno.TratamientoId));
        }

        return resultado;
    }

    public async Task<List<IntervaloDeTiempo>> ObtenerHuecosDisponiblesAsync(
        int odontologoId,
        DateTime fecha,
        TimeSpan duracionRequerida)
    {
        IEnumerable<Turno> todos = await _repositorioTurnos.ObtenerTodos();
        List<Turno> turnosDelDia = new List<Turno>();

        foreach (Turno turno in todos)
        {
            if (turno.OdontologoId == odontologoId &&
                turno.Estado != EstadoTurno.Cancelado &&
                turno.Horario.Inicio.Date == fecha.Date)
            {
                turnosDelDia.Add(turno);
            }
        }

        turnosDelDia.Sort(CompararTurnosPorInicio);

        List<IntervaloDeTiempo> huecosEncontrados = new List<IntervaloDeTiempo>();

        DateTime inicioJornada = fecha.Date.AddHours(8);
        DateTime finJornada = fecha.Date.AddHours(18);
        DateTime puntero = inicioJornada;

        foreach (Turno turno in turnosDelDia)
        {
            if (puntero < turno.Horario.Inicio)
            {
                IntervaloDeTiempo hueco = new IntervaloDeTiempo(puntero, turno.Horario.Inicio);

                if (hueco.Duracion >= duracionRequerida)
                {
                    huecosEncontrados.Add(hueco);
                }
            }

            if (turno.Horario.Fin > puntero)
            {
                puntero = turno.Horario.Fin;
            }
        }

        if (puntero < finJornada)
        {
            IntervaloDeTiempo huecoFinal = new IntervaloDeTiempo(puntero, finJornada);

            if (huecoFinal.Duracion >= duracionRequerida)
            {
                huecosEncontrados.Add(huecoFinal);
            }
        }

        return huecosEncontrados;
    }

    public IntervaloDeTiempo? SeleccionarMejorHuecoBestFit(
        List<IntervaloDeTiempo> huecos,
        TimeSpan duracionRequerida)
    {
        IntervaloDeTiempo? mejorHueco = null;
        TimeSpan menorResiduo = TimeSpan.MaxValue;

        for (int i = 0; i < huecos.Count; i++)
        {
            TimeSpan residuo = huecos[i].Duracion - duracionRequerida;

            if (residuo < TimeSpan.Zero)
                continue;

            if (residuo == TimeSpan.Zero)
            {
                return new IntervaloDeTiempo(
                    huecos[i].Inicio,
                    huecos[i].Inicio.Add(duracionRequerida));
            }

            if (residuo < menorResiduo)
            {
                menorResiduo = residuo;
                mejorHueco = huecos[i];
            }
        }

        if (mejorHueco != null)
        {
            return new IntervaloDeTiempo(
                mejorHueco.Inicio,
                mejorHueco.Inicio.Add(duracionRequerida));
        }

        return null;
    }

    private static int CompararTurnosPorInicio(Turno primero, Turno segundo)
    {
        return primero.Horario.Inicio.CompareTo(segundo.Horario.Inicio);
    }
}
