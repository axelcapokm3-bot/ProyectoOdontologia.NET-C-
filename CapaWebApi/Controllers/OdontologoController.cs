using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Odontologo,Recepcionista,Paciente")]
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OdontologoOutputDto>> ObtenerPorId(int id)
        {
            var dto = await _odontologoService.ObtenerOdontologoPorIdAsync(id);
            return Ok(dto);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<OdontologoOutputDto>>> Buscar([FromQuery] string texto)
        {
            var resultados = await _odontologoService.BuscarOdontologosAsync(texto);
            return Ok(resultados);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Guardar([FromBody] OdontologoInputDto odontologo)
        {
            if (odontologo == null) return BadRequest();

            await _odontologoService.RegistrarOdontologoAsync(odontologo);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] OdontologoInputDto odontologo)
        {
            if (odontologo == null) return BadRequest();

            await _odontologoService.ActualizarOdontologoAsync(id, odontologo);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _odontologoService.EliminarOdontologoAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}
