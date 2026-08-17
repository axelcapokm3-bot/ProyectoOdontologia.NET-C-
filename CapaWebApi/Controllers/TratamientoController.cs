using Microsoft.AspNetCore.Mvc;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TratamientoController : ControllerBase
    {
        private readonly ITratamientoService _tratamientoService;

        public TratamientoController(ITratamientoService tratamientoService)
        {
            _tratamientoService = tratamientoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TratamientoDtoOutput>>> ObtenerTodos()
        {
            var lista = await _tratamientoService.ObtenerTodosLosTratamientosAsync();
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TratamientoDtoOutput>> ObtenerPorId(int id)
        {
            try
            {
                var dto = await _tratamientoService.ObtenerTratamientoPorIdAsync(id);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<TratamientoDtoOutput>>> Buscar([FromQuery] string texto)
        {
            var resultados = await _tratamientoService.BuscarTratamientosAsync(texto);
            return Ok(resultados);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] TratamientoDtoInput tratamiento)
        {
            if (tratamiento == null) return BadRequest();
            await _tratamientoService.RegistrarTratamientoAsync(tratamiento);
            return StatusCode(201);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] TratamientoDtoInput tratamiento)
        {
            if (tratamiento == null) return BadRequest();
            try
            {
                await _tratamientoService.ActualizarTratamientoAsync(id, tratamiento);
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
            var eliminado = await _tratamientoService.EliminarTratamientoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
