namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IInsumosService
{
    Task<IEnumerable<InsumoDtosOutput>> ObtenerTodosLosInsumosAsync();
    Task<InsumoDtosOutput?> ObtenerInsumoPorIdAsync(int id);
    Task<List<InsumoDtosOutput>> BuscarInsumosAsync(string criterio);
    Task RegistrarInsumoAsync(InsumoDtosInput nuevoInsumo);
    Task ActualizarInsumoAsync(int id, InsumoDtosInput insumoEditado);
    Task<bool> EliminarInsumoAsync(int id);
}
