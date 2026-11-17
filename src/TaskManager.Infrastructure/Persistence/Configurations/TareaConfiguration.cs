using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{

    private static Tarea[] Seed() =>
    [
        T(1,  "Configurar el proyecto",     "Estructura inicial de la solución", EstadoTarea.Hecha,     new(2026, 12, 1),  1),
        T(2,  "Diseñar el modelo de datos", "Entidades y relaciones",            EstadoTarea.Hecha,     new(2026, 12, 3),  1),
        T(3,  "Implementar la API",         "Controllers y servicios",           EstadoTarea.EnCurso,   new(2026, 12, 10), 2),
        T(4,  "Crear las páginas Razor",    "Listado, detalle y formularios",    EstadoTarea.EnCurso,   new(2026, 12, 12), 3),
        T(5,  "Escribir tests",             "Servicios e integración",           EstadoTarea.Pendiente, new(2026, 12, 15), 2),
        T(6,  "Redactar el README",         null,                                EstadoTarea.Pendiente, new(2026, 12, 16), 1),
        T(7,  "Revisar validaciones",       "DataAnnotations en los DTOs",       EstadoTarea.Pendiente, new(2026, 12, 18), 3),
        T(8,  "Probar paginación",          "Con más de 10 tareas",              EstadoTarea.Pendiente, new(2026, 12, 19), 3),
        T(9,  "Revisar códigos HTTP",       "400, 404, 409",                     EstadoTarea.Pendiente, new(2026, 12, 20), 2),
        T(10, "Preparar demo",              null,                                EstadoTarea.Pendiente, new(2026, 12, 22), 1),
        T(11, "Limpiar código",             "Quitar usings y archivos sobrantes", EstadoTarea.Pendiente, new(2026, 12, 23), 2),
        T(12, "Subir versión final",        "Último commit y push",              EstadoTarea.Pendiente, new(2026, 12, 28), 1)
    ];

    private static Tarea T(int id, string titulo, string? descripcion,
        EstadoTarea estado, DateOnly fecha, int responsableId) => new()
        {
            Id = id,
            Titulo = titulo,
            Descripcion = descripcion,
            Estado = estado,
            FechaLimite = fecha,
            ResponsableId = responsableId
        };

    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Descripcion).HasMaxLength(1000);
        builder.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.FechaLimite).IsRequired();

        builder.HasOne(t => t.Responsable)
               .WithMany(r => r.Tareas)
               .HasForeignKey(t => t.ResponsableId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Estado);

        builder.HasData(Seed());
    }
}