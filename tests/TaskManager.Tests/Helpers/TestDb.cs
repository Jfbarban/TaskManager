using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Tests.Helpers;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();

        // EnsureCreated incluye los datos de ejemplo (HasData); los quitamos
        // para que cada test parta de una base vacía y cree lo que necesite.
        Context.Tareas.ExecuteDelete();
        Context.Responsables.ExecuteDelete();
    }

    public async Task<Responsable> AddResponsableAsync(string nombre = "Responsable de prueba")
    {
        var responsable = new Responsable { Nombre = nombre };
        Context.Responsables.Add(responsable);
        await Context.SaveChangesAsync();
        return responsable;
    }

    public async Task<Tarea> AddTareaAsync(
        int responsableId,
        string titulo = "Tarea de prueba",
        EstadoTarea estado = EstadoTarea.Pendiente,
        DateOnly? fechaLimite = null)
    {
        var tarea = new Tarea
        {
            Titulo = titulo,
            Estado = estado,
            FechaLimite = fechaLimite ?? DateOnly.FromDateTime(DateTime.Today).AddDays(7),
            ResponsableId = responsableId
        };
        Context.Tareas.Add(tarea);
        await Context.SaveChangesAsync();
        return tarea;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}