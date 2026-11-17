using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.Interfaces;

namespace TaskManager.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/responsables")]
[Produces("application/json")]
public class ResponsablesController : ControllerBase
{
    private readonly IResponsableService _service;

    public ResponsablesController(IResponsableService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResponsableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResponsableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => Ok(await _service.ObtenerAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(ResponsableDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(ResponsableCreateDto dto, CancellationToken ct)
    {
        var creado = await _service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }
}