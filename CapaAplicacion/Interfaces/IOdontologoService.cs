namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOdontologoService
{
    Task<IEnumerable<OdontologoOutputDto>> ObtenerTodosLosOdontologosAsync();
    Task<OdontologoOutputDto> ObtenerOdontologoPorIdAsync(int id);
    Task<List<OdontologoOutputDto>> BuscarOdontologosAsync(string criterio);
    Task RegistrarOdontologoAsync(OdontologoInputDto nuevoOdontologo);
    Task ActualizarOdontologoAsync(int id, OdontologoInputDto odontologoEditado);
    Task<bool> EliminarOdontologoAsync(int id);
}