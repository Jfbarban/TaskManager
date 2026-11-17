using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class TareaService : ITareaService
{
    private readonly IAppDbContext _db;

    public TareaService(IAppDbContext db) => _db = db;

    public async Task<PagedResult<TareaDto>> ListarAsync(TareaFiltroDto filtro, CancellationToken ct = default)
    {
        var query = _db.Tareas.AsNoTracking().AsQueryable();

        if (filtro.Estado.HasValue)
            query = query.Where(t => t.Estado == filtro.Estado.Value);

        if (filtro.ResponsableId.HasValue)
            query = query.Where(t => t.ResponsableId == filtro.ResponsableId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.FechaLimite)
            .ThenBy(t => t.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                FechaLimite = t.FechaLimite,
                ResponsableId = t.ResponsableId,
                ResponsableNombre = t.Responsable.Nombre
            })
            .ToListAsync(ct);

        return new PagedResult<TareaDto>
        {
            Items = items,
            Pagina = filtro.Pagina,
            TamanoPagina = filtro.TamanoPagina,
            TotalItems = total
        };
    }

    public async Task<TareaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var tarea = await _db.Tareas
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = t.Estado,
                FechaLimite = t.FechaLimite,
                ResponsableId = t.ResponsableId,
                ResponsableNombre = t.Responsable.Nombre
            })
            .FirstOrDefaultAsync(ct);

        return tarea ?? throw new NotFoundException($"No existe la tarea con id {id}.");
    }

    public async Task<TareaDto> CrearAsync(TareaCreateDto dto, CancellationToken ct = default)
    {
        await ValidarResponsableAsync(dto.ResponsableId, ct);

        var tarea = new Tarea
        {
            Titulo = dto.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Estado = dto.Estado,
            FechaLimite = dto.FechaLimite!.Value,
            ResponsableId = dto.ResponsableId
        };

        _db.Tareas.Add(tarea);
        await _db.SaveChangesAsync(ct);

        return await ObtenerAsync(tarea.Id, ct);
    }

    public async Task<TareaDto> ActualizarAsync(int id, TareaUpdateDto dto, CancellationToken ct = default)
    {
        var tarea = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"No existe la tarea con id {id}.");

        await ValidarResponsableAsync(dto.ResponsableId, ct);

        tarea.Titulo = dto.Titulo.Trim();
        tarea.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        tarea.Estado = dto.Estado;
        tarea.FechaLimite = dto.FechaLimite!.Value;
        tarea.ResponsableId = dto.ResponsableId;

        await _db.SaveChangesAsync(ct);

        return await ObtenerAsync(tarea.Id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var tarea = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"No existe la tarea con id {id}.");

        _db.Tareas.Remove(tarea);
        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidarResponsableAsync(int responsableId, CancellationToken ct)
    {
        var existe = await _db.Responsables.AnyAsync(r => r.Id == responsableId, ct);
        if (!existe)
            throw new BusinessValidationException(
                nameof(TareaCreateDto.ResponsableId),
                $"El responsable con id {responsableId} no existe.");
    }
}