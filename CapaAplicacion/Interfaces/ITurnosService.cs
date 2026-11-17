namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITurnosService
{
    Task<IEnumerable<TurnoDtoOutput>> ObtenerTodosLosTurnosAsync();
    Task<TurnoDtoOutput> ObtenerTurnoPorIdAsync(int id);
    Task<List<TurnoDtoOutput>> BuscarTurnosAsync(string criterio);
    Task RegistrarTurnoAsync(TurnoDtoInput nuevoTurno);
    Task ActualizarTurnoAsync(int id, TurnoDtoInput turnoEditado);
    Task<bool> EliminarTurnoAsync(int id);

    Task<int> RegistrarTurnoAsync(CrearTurnoRequestDto nuevoTurno, CancellationToken cancellationToken = default);

    Task<List<IntervaloTiempoDto>> ConsultarHuecosDisponiblesAsync(ConsultarHuecosRequestDto request, CancellationToken cancellationToken = default);

    Task CancelarTurnoAsync(int id, CancellationToken cancellationToken = default);

    Task CompletarTurnoAsync(int id, CancellationToken cancellationToken = default);
}
