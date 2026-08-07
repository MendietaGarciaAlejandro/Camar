using Camar.Domain.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Camar.Infrastructure.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        // El enum se guarda como su valor numerico, por eso los valores del enum son explicitos.
        builder.Property(r => r.Type)
            .IsRequired();

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();
    }
}
