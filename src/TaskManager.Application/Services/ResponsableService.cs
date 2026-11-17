using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class ResponsableService : IResponsableService
{
    private readonly IAppDbContext _db;

    public ResponsableService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ResponsableDto>> ListarAsync(CancellationToken ct = default)
    {
        return await _db.Responsables
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new ResponsableDto { Id = r.Id, Nombre = r.Nombre })
            .ToListAsync(ct);
    }

    public async Task<ResponsableDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var responsable = await _db.Responsables
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ResponsableDto { Id = r.Id, Nombre = r.Nombre })
            .FirstOrDefaultAsync(ct);

        return responsable ?? throw new NotFoundException($"No existe el responsable con id {id}.");
    }

    public async Task<ResponsableDto> CrearAsync(ResponsableCreateDto dto, CancellationToken ct = default)
    {
        var responsable = new Responsable { Nombre = dto.Nombre.Trim() };

        _db.Responsables.Add(responsable);
        await _db.SaveChangesAsync(ct);

        return new ResponsableDto { Id = responsable.Id, Nombre = responsable.Nombre };
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var responsable = await _db.Responsables.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException($"No existe el responsable con id {id}.");

        var totalTareas = await _db.Tareas.CountAsync(t => t.ResponsableId == id, ct);
        if (totalTareas > 0)
            throw new ConflictException(
                $"No se puede eliminar a '{responsable.Nombre}': tiene {totalTareas} tarea(s) asignada(s).");

        _db.Responsables.Remove(responsable);
        await _db.SaveChangesAsync(ct);
    }
}