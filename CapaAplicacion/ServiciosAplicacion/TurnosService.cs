namespace ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;

using global::CapaAplicacion.Interfaces;
using global::ProyectoOdontologia.CapaAplicacion.DtosInput;
using global::ProyectoOdontologia.CapaAplicacion.DtosOutput;
using global::CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class TurnosService : ITurnosService
{
    private readonly IRepositorioTurnos _repositorioTurnos;

    public TurnosService(IRepositorioTurnos repositorioTurnos)
    {
        _repositorioTurnos = repositorioTurnos;
    }

    public Task RegistrarTurnoAsync(TurnoDtoInput turno)
    {
        if (turno.FechaHora < DateTime.Now)
        {
            throw new ArgumentException("La fecha y hora del turno no puede ser en el pasado.");
        }

        int nuevoId = autoincremental();

        var nuevoTurno = new Turnos(
            nuevoId,
            turno.FechaHora,
            turno.PacienteId,
            turno.OdontologoId,
            turno.TratamientoId
        );

        _repositorioTurnos.AgregarTurno(nuevoTurno);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<TurnoDtoOutput>> ObtenerTodosLosTurnosAsync()
    {
        var turnos = _repositorioTurnos.ObtenerTodos();
        var resultado = new List<TurnoDtoOutput>();

        foreach (var t in turnos)
        {
            var dto = new TurnoDtoOutput(
                Guid.Parse(t.Id.ToString().PadLeft(32, '0')),
                t.FechaHora,
                t.PacienteId,
                t.OdontologoId,
                t.TratamientoId
            );
            resultado.Add(dto);
        }

        return Task.FromResult<IEnumerable<TurnoDtoOutput>>(resultado);
    }

    public Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id)
    {
        var t = _repositorioTurnos.ObtenerTurnoPorId(id);
        if (t == null) throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

        var dto = new TurnoDtoOutput(
            Guid.Parse(t.Id.ToString().PadLeft(32, '0')),
            t.FechaHora,
            t.PacienteId,
            t.OdontologoId,
            t.TratamientoId
        );

        return Task.FromResult<TurnoDtoOutput>(dto);
    }

    public Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado)
    {
        var existe = _repositorioTurnos.ObtenerTurnoPorId(id);
        if (existe == null) throw new KeyNotFoundException($"No se encontró el turno con ID {id}");

        var turnoModificado = new Turnos(
            id,
            turnoEditado.FechaHora,
            turnoEditado.PacienteId,
            turnoEditado.OdontologoId,
            turnoEditado.TratamientoId
        );

        _repositorioTurnos.ActualizarTurno(turnoModificado);

        return Task.CompletedTask;
    }

    public Task<bool> EliminarTurnoAsync(int id)
    {
        var existe = _repositorioTurnos.ObtenerTurnoPorId(id);
        if (existe == null) return Task.FromResult(false);

        _repositorioTurnos.EliminarTurno(id);
        return Task.FromResult(true);
    }

    public int autoincremental()
    {
        var lista = _repositorioTurnos.ObtenerTodos();
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
