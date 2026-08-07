using Camar.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Camar.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        // El email identifica al usuario al iniciar sesion: no puede repetirse.
        // Se guarda normalizado en minusculas, asi que basta con un indice unico normal.
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.MembershipPlan).IsRequired();
        builder.Property(u => u.Role).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
