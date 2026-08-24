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
//el semaforo decide como un peajo cuantos hilos o subprocesos ejecutamos a la vez I/O 
    private static readonly SemaphoreSlim _semaforo = new SemaphoreSlim(1,1);
    public TurnosService(IRepositorioTurnos repositorioTurnos)
    {
        _repositorioTurnos = repositorioTurnos;
    }

   public async Task RegistrarTurnoAsync(TurnoDtoInput turnoDto)
{
    if (turnoDto == null)
        throw new ArgumentNullException(nameof(turnoDto));

    // 1. Entrada al semáforo de forma asíncrona (Sincronización thread-safe)
    await _semaforo.WaitAsync();

    try
    {
        // 2. Traemos los turnos del Odontólogo desde el Repositorio
        var turnosOdontologo = await _repositorioTurnos.ObtenerTodos();

        // 3. Generamos el ID atómico
        int nuevoId = _repositorioTurnos.IdAutoIncrementoSincrono();

        // 4. Instanciamos la entidad Turno
        var nuevoTurno = new Turno(
            nuevoId,
            turnoDto.FechaHoraInicio,
            turnoDto.FechaHoraFin,
            turnoDto.PacienteId,
            turnoDto.OdontologoId,
            turnoDto.TratamientoId
        );

        // 5. Invocación DIRECTA a la regla de tu entidad Turno
        bool esAgendaValida = Turno.EsValidoLaAgenda(turnosOdontologo, nuevoTurno);

        if (!esAgendaValida)
        {
            throw new InvalidOperationException("El odontólogo ya posee una cita activa agendada en ese rango horario.");
        }

        // 6. Persistencia
        await _repositorioTurnos.AgregarTurno(nuevoTurno);
    }
    finally
    {
        _semaforo.Release();
    }
}
    public async Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id)
    {
        var t = await _repositorioTurnos.ObtenerTurnoPorId(id);
        if (t == null) throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

        return new TurnoDtoOutput(t.Id, t.Horario.Inicio, t.PacienteId, t.OdontologoId, t.TratamientoId);
    }

    public async Task<List<TurnoDtoOutput>> BuscarTurnosAsync(string criterio)
    {
        List<Turno> turnos = await _repositorioTurnos.BuscarTurnos(criterio);
        var resultado = new List<TurnoDtoOutput>();

        foreach (var t in turnos)
        {
            resultado.Add(new TurnoDtoOutput(t.Id, t.Horario.Inicio, t.PacienteId, t.OdontologoId, t.TratamientoId));
        }

        return resultado;
    }

    public async Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado)
    {
        await _semaforo.WaitAsync();

        try
        {
            var existe = await _repositorioTurnos.ObtenerTurnoPorId(id);
            if (existe == null) throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

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
        finally
        {
            _semaforo.Release();
        }
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



}