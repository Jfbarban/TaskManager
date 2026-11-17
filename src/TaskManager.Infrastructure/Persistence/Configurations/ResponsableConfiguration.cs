using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class ResponsableConfiguration : IEntityTypeConfiguration<Responsable>
{
    public void Configure(EntityTypeBuilder<Responsable> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).IsRequired().HasMaxLength(100);
        builder.HasData(
    new Responsable { Id = 1, Nombre = "Ana García" },
    new Responsable { Id = 2, Nombre = "Luis Pérez" },
    new Responsable { Id = 3, Nombre = "María López" });
    }
}