using Microsoft.AspNetCore.Mvc;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OdontologoController : ControllerBase
    {
        private readonly IOdontologoService _odontologoService;

        public OdontologoController(IOdontologoService odontologoService)
        {
            _odontologoService = odontologoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OdontologoOutputDto>>> ObtenerTodos()
        {
            var lista = await _odontologoService.ObtenerTodosLosOdontologosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OdontologoOutputDto>> ObtenerPorId(int id)
        {
            try
            {
                var dto = await _odontologoService.ObtenerOdontologoPorIdAsync(id);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Guardar([FromBody] OdontologoInputDto odontologo)
        {
            if (odontologo == null) return BadRequest();
            await _odontologoService.RegistrarOdontologoAsync(odontologo);
            return StatusCode(201);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] OdontologoInputDto odontologo)
        {
            if (odontologo == null) return BadRequest();
            try
            {
                await _odontologoService.ActualizarOdontologoAsync(id, odontologo);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _odontologoService.EliminarOdontologoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
