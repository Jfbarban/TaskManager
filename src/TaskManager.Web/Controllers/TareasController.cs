using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Application.Interfaces;

namespace TaskManager.Web.Controllers;

[ApiController]
[Route("api/tareas")]
[Produces("application/json")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _service;

    public TareasController(ITareaService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TareaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] TareaFiltroDto filtro, CancellationToken ct)
        => Ok(await _service.ListarAsync(filtro, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => Ok(await _service.ObtenerAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(TareaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(TareaCreateDto dto, CancellationToken ct)
    {
        var creada = await _service.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TareaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, TareaUpdateDto dto, CancellationToken ct)
        => Ok(await _service.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _service.EliminarAsync(id, ct);
        return NoContent();
    }
}