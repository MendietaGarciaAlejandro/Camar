using Camar.Application.Abstractions;
using Camar.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CamarDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
}
