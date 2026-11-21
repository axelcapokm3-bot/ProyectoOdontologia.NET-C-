using CapaAplicacion.Seguridad.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoOdontologia.CapaAplicacion.DtosInput;

namespace CapaWebApi.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioAdminService _usuarioAdminService;

    public UsuariosController(IUsuarioAdminService usuarioAdminService)
    {
        _usuarioAdminService = usuarioAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await _usuarioAdminService.ListarUsuariosAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var usuario = await _usuarioAdminService.ObtenerPorIdAsync(id);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPatch("{id:int}/rol")]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolRequestDto dto)
    {
        await _usuarioAdminService.CambiarRolUsuarioAsync(id, dto.Rol);
        return NoContent();
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] bool estaActivo)
    {
        await _usuarioAdminService.CambiarEstadoAsync(id, estaActivo);
        return NoContent();
    }
}
