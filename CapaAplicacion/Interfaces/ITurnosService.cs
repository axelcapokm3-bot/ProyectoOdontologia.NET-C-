namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITurnosService
{
    Task<IEnumerable<TurnoDtoOutput>> ObtenerTodosLosTurnosAsync();
    Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id);
    Task<List<TurnoDtoOutput>> BuscarTurnosAsync(string criterio);
    Task RegistrarTurnoAsync(TurnoDtoInput nuevoTurno);
    Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado);
    Task<bool> EliminarTurnoAsync(int id);
}