using Camar.Application.Abstractions;
using Camar.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CamarDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    // El email se guarda normalizado en minusculas, asi que se busca igual.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), ct);

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        return db.SaveChangesAsync(ct);
    }
}
