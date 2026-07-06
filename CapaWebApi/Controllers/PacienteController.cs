using Microsoft.AspNetCore.Mvc;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacienteController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;
        //inyeccion
        public PacienteController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteDtoOutput>>> ObtenerTodos()
        {
            var pacientes = await _pacienteService.ObtenerTodosLosPacientesAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDtoOutput>> ObtenerPorId(int id)
        {
            var paciente = await _pacienteService.ObtenerPacientePorIdAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }
            return Ok(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] PacienteDtoInput pacienteDtoInput)
        {
            if (pacienteDtoInput == null)
            {
                return BadRequest();
            }

            await _pacienteService.RegistrarPacienteAsync(pacienteDtoInput);
            return StatusCode(201);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] PacienteDtoInput pacienteDtoInput)
        {
            if (pacienteDtoInput == null)
            {
                return BadRequest();
            }

            await _pacienteService.ActualizarPacienteAsync(id, pacienteDtoInput);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _pacienteService.EliminarPacienteAsync(id);
            if (!eliminado)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}