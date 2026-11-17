using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Tarea> Tareas { get; }
    DbSet<Responsable> Responsables { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}