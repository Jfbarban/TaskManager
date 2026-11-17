using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
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
    }
}