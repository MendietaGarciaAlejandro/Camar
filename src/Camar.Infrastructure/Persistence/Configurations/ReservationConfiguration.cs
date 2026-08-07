using Camar.Domain.Reservations;
using Camar.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Camar.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);

        // tstzrange para que la constraint de exclusion pueda operar con && sobre el rango
        builder.Property(r => r.Period)
            .HasConversion(new PeriodConverter())
            .HasColumnType("tstzrange")
            .HasColumnName("period")
            .IsRequired();

        builder.Property(r => r.ResourceId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.Property(r => r.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        // nullables: solo se rellenan al cancelar
        builder.Property(r => r.CancelledAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(r => r.RefundAmount)
            .HasPrecision(10, 2);

        builder.HasIndex(r => r.ResourceId);
    }
}
