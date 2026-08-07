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

        // 1) El Period: aqui va el converter y el tipo de columna
        builder.Property(r => r.Period)
            .HasConversion(new PeriodConverter())
            .HasColumnType("tstzrange")
            .HasColumnName("period")
            .IsRequired();

        // 2) ResourceId, UserId, Status, CreatedAt -> todas .IsRequired()
        //    (escribelas tu, siguiendo el patron de ResourceConfiguration)
        builder.Property(r => r.ResourceId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // 3) CancelledAt -> NO lleva IsRequired: es nullable por diseño
        builder.Property(r => r.CancelledAt)
            .HasColumnType("timestamp with time zone");

        // 4) Indice para buscar las reservas de un recurso:
        builder.HasIndex(r => r.ResourceId);
    }
}