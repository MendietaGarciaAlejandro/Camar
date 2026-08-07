using Camar.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Camar.Infrastructure.Persistence.Configurations;

public sealed class BlockedDayConfiguration : IEntityTypeConfiguration<BlockedDay>
{
    public void Configure(EntityTypeBuilder<BlockedDay> builder)
    {
        builder.ToTable("blocked_days");

        builder.HasKey(b => b.Id);

        // Un dia solo puede estar bloqueado una vez.
        builder.HasIndex(b => b.Date).IsUnique();

        builder.Property(b => b.Reason)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.CreatedAt).IsRequired();
    }
}
