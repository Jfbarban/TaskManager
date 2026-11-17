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
    }
}