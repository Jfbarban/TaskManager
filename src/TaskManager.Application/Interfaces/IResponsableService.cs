using TaskManager.Application.DTOs.Responsables;

namespace TaskManager.Application.Interfaces;

public interface IResponsableService
{
    Task<IReadOnlyList<ResponsableDto>> ListarAsync(CancellationToken ct = default);
    Task<ResponsableDto> ObtenerAsync(int id, CancellationToken ct = default);
    Task<ResponsableDto> CrearAsync(ResponsableCreateDto dto, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}