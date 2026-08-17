using Microsoft.AspNetCore.Mvc;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly ITurnosService _turnosService;

        public TurnosController(ITurnosService turnosService)
        {
            _turnosService = turnosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnoDtoOutput>>> ObtenerTodos()
        {
            var lista = await _turnosService.ObtenerTodosLosTurnosAsync();
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TurnoDtoOutput>> ObtenerPorId(int id)
        {
            try
            {
                var dto = await _turnosService.ObtenerTurnoPorIdAsync(id);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<TurnoDtoOutput>>> Buscar([FromQuery] string texto)
        {
            var resultados = await _turnosService.BuscarTurnosAsync(texto);
            return Ok(resultados);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] TurnoDtoInput turno)
        {
            if (turno == null) return BadRequest();
            await _turnosService.RegistrarTurnoAsync(turno);
            return StatusCode(201);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] TurnoDtoInput turno)
        {
            if (turno == null) return BadRequest();
            try
            {
                await _turnosService.ActualizarTurnoAsync(id, turno);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _turnosService.EliminarTurnoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
