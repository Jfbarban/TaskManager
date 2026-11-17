using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tareas;

namespace TaskManager.Application.Interfaces;

public interface ITareaService
{
    Task<PagedResult<TareaDto>> ListarAsync(TareaFiltroDto filtro, CancellationToken ct = default);
    Task<TareaDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<TareaDto> CrearAsync(TareaCreateDto dto, CancellationToken ct = default);
    Task<TareaDto> ActualizarAsync(int id, TareaUpdateDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}