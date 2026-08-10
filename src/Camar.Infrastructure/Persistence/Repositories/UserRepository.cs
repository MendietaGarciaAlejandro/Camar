using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CamarDbContext db) : IUserRepository
{
    // 23505 = unique_violation. Aqui la lanzan el indice del email y el del documento fiscal.
    private const string UniqueViolation = "23505";

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    // El email se guarda normalizado en minusculas, asi que se busca igual.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (BuscarViolacionDeUnicidad(ex) is { } violacion)
        {
            // El alta comprueba antes si el email existe, pero entre esa consulta y el
            // insert hay una ventana en la que otro registro puede colarse. Igual que con
            // las reservas, la garantia de verdad la pone el indice de la base de datos y
            // aqui solo se traduce a un error que el cliente pueda entender.
            db.Entry(user).State = EntityState.Detached;

            throw new ConflictException(Mensaje(violacion.ConstraintName));
        }
    }

    /// <summary>
    /// EF envuelve el fallo de Postgres, asi que hay que recorrer la cadena de excepciones
    /// hasta encontrar la de verdad.
    /// </summary>
    private static PostgresException? BuscarViolacionDeUnicidad(Exception excepcion)
    {
        for (var actual = excepcion; actual is not null; actual = actual.InnerException)
        {
            if (actual is PostgresException { SqlState: UniqueViolation } pg) return pg;
        }

        return null;
    }

    private static string Mensaje(string? constraint) => constraint switch
    {
        "ix_users_tax_id" => "Ese documento fiscal ya esta registrado.",
        "ix_users_email" => "Ese email ya esta registrado.",
        _ => "Ya existe un socio con esos datos.",
    };
}
