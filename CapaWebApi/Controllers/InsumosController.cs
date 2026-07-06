using Microsoft.AspNetCore.Mvc;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsumosControllers : ControllerBase
    {
        private readonly IInsumosService _insumosService;

        public InsumosControllers(IInsumosService insumosService)
        {
            _insumosService = insumosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsumoDtosOutput>>> ObtenerTodos()
        {
            var lista = await _insumosService.ObtenerTodosLosInsumosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InsumoDtosOutput>> ObtenerPorId(int id)
        {
            var dto = await _insumosService.ObtenerInsumoPorIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] InsumoDtosInput insumo)
        {
            if (insumo == null) return BadRequest();
            await _insumosService.RegistrarInsumoAsync(insumo);
            return StatusCode(201);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] InsumoDtosInput insumo)
        {
            if (insumo == null) return BadRequest();
            await _insumosService.ActualizarInsumoAsync(id, insumo);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _insumosService.EliminarInsumoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
