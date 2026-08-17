namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITratamientoService
{
    Task<IEnumerable<TratamientoDtoOutput>> ObtenerTodosLosTratamientosAsync();
    Task<TratamientoDtoOutput> ObtenerTratamientoPorIdAsync(int id);
    Task<List<TratamientoDtoOutput>> BuscarTratamientosAsync(string criterio);
    Task RegistrarTratamientoAsync(TratamientoDtoInput nuevoTratamiento);
    Task ActualizarTratamientoAsync(int id, TratamientoDtoInput tratamientoEditado);
    Task<bool> EliminarTratamientoAsync(int id);
}