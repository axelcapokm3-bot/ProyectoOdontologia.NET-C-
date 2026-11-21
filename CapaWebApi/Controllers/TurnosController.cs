using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using ProyectoOdontologia.CapaAplicacion.DtosOutput;
using System.Security.Claims;
using System.Threading;

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

        private int ObtenerUsuarioIdDelToken()
        {
            string? valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");
            }

            return int.Parse(valor);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista")]
        public async Task<ActionResult<IEnumerable<TurnoDtoOutput>>> ObtenerTodos()
        {
            var lista = await _turnosService.ObtenerTodosLosTurnosAsync();
            return Ok(lista);
        }

        [HttpGet("mis-turnos")]
        [Authorize(Roles = "Paciente")]
        public async Task<ActionResult<IEnumerable<TurnoDtoOutput>>> ObtenerMisTurnos()
        {
            int usuarioId = ObtenerUsuarioIdDelToken();
            var turnos = await _turnosService.ObtenerTurnosDelPacienteAsync(usuarioId);
            return Ok(turnos);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista,Paciente")]
        public async Task<ActionResult<TurnoDtoOutput>> ObtenerPorId(int id)
        {
            var dto = await _turnosService.ObtenerTurnoPorIdAsync(id);
            return Ok(dto);
        }

        [HttpGet("buscar")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista")]
        public async Task<ActionResult<List<TurnoDtoOutput>>> Buscar([FromQuery] string texto)
        {
            var resultados = await _turnosService.BuscarTurnosAsync(texto);
            return Ok(resultados);
        }

        [HttpGet("huecos")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista,Paciente")]
        public async Task<ActionResult<List<IntervaloTiempoDto>>> ConsultarHuecos(
            [FromQuery] int odontologoId,
            [FromQuery] DateTime fecha,
            [FromQuery] int duracionMinutos,
            CancellationToken cancellationToken)
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

        [HttpPost]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista,Paciente")]
        public async Task<ActionResult<int>> Guardar([FromBody] CrearTurnoRequestDto turno, CancellationToken cancellationToken)
        {
            if (turno == null) return BadRequest();

            string? rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rolUsuario != null && rolUsuario.Equals("Paciente", System.StringComparison.OrdinalIgnoreCase))
            {
                int usuarioId = ObtenerUsuarioIdDelToken();
                int id = await _turnosService.RegistrarTurnoParaPacienteAsync(turno, usuarioId, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, id);
            }

            int idTurno = await _turnosService.RegistrarTurnoAsync(turno, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, idTurno);
        }

        [HttpPatch("{id:int}/cancelar")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista,Paciente")]
        public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
        {
            string? rolUsuario = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rolUsuario != null && rolUsuario.Equals("Paciente", System.StringComparison.OrdinalIgnoreCase))
            {
                int usuarioId = ObtenerUsuarioIdDelToken();
                await _turnosService.CancelarTurnoDelPacienteAsync(id, usuarioId, cancellationToken);
                return NoContent();
            }

            await _turnosService.CancelarTurnoAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/completar")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista")]
        public async Task<IActionResult> Completar(int id, CancellationToken cancellationToken)
        {
            await _turnosService.CompletarTurnoAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Odontologo,Recepcionista")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] TurnoDtoInput turno)
        {
            if (turno == null) return BadRequest();
            await _turnosService.ActualizarTurnoAsync(id, turno);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _turnosService.EliminarTurnoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
