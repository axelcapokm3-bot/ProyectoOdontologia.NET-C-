using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;

namespace CapaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Odontologo,Recepcionista")]
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

        [HttpGet("huecos")]
        public async Task<ActionResult<List<IntervaloTiempoDto>>> ConsultarHuecos(
            [FromQuery] int odontologoId,
            [FromQuery] DateTime fecha,
            [FromQuery] int duracionMinutos,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new ConsultarHuecosRequestDto
                {
                    OdontologoId = odontologoId,
                    Fecha = fecha,
                    DuracionMinutos = duracionMinutos
                };

                var huecos = await _turnosService.ConsultarHuecosDisponiblesAsync(request, cancellationToken);
                return Ok(huecos);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Parámetros inválidos", Detail = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Parámetros inválidos", Detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> Guardar([FromBody] CrearTurnoRequestDto turno, CancellationToken cancellationToken)
        {
            if (turno == null) return BadRequest();

            try
            {
                int id = await _turnosService.RegistrarTurnoAsync(turno, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, id);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Conflicto de agenda", Detail = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Datos inválidos", Detail = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Recurso no encontrado", Detail = ex.Message });
            }
        }

        [HttpPatch("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _turnosService.CancelarTurnoAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Conflicto de estado", Detail = ex.Message });
            }
        }

        [HttpPatch("{id:int}/completar")]
        public async Task<IActionResult> Completar(int id, CancellationToken cancellationToken)
        {
            await _turnosService.CompletarTurnoAsync(id, cancellationToken);
            return NoContent();
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
