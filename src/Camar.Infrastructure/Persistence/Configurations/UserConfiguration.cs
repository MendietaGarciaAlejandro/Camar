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

        // Los objetos de valor se guardan como su texto y se reconstruyen al leer, que es
        // cuando se vuelven a validar. Asi la base de datos no puede devolver un documento
        // fiscal que no cumpla las reglas.
        builder.Property(u => u.TaxId)
            .HasConversion(taxId => taxId.Value, texto => new TaxId(texto))
            .HasColumnName("tax_id")
            .HasMaxLength(9)
            .IsRequired();

        // Dos socios no pueden compartir documento fiscal: seria la misma persona.
        builder.HasIndex(u => u.TaxId).IsUnique();

        builder.Property(u => u.Phone)
            .HasConversion(phone => phone.Value, texto => new PhoneNumber(texto))
            .HasColumnName("phone")
            .HasMaxLength(9)
            .IsRequired();

        builder.Property(u => u.PostalCode)
            .HasConversion(codigo => codigo.Value, texto => new PostalCode(texto))
            .HasColumnName("postal_code")
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(u => u.BankAccount)
            .HasConversion(
                cuenta => cuenta!.Value.Value,
                texto => new BankAccount(texto))
            .HasColumnName("bank_account")
            .HasMaxLength(34);
    }
}
