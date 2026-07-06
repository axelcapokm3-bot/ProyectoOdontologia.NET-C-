namespace CapaAplicacion.Interfaces;

using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPacienteService
{
    Task<IEnumerable<PacienteDtoOutput>> ObtenerTodosLosPacientesAsync();
    Task<PacienteDtoOutput> ObtenerPacientePorIdAsync(int id);
    Task RegistrarPacienteAsync(PacienteDtoInput nuevoPaciente);
    Task ActualizarPacienteAsync(int id, PacienteDtoInput pacienteEditado);
    Task<bool> EliminarPacienteAsync(int id);
}