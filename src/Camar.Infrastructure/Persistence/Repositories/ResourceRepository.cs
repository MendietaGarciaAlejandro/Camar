using Camar.Application.Abstractions;
using Camar.Domain.Resources;
using Microsoft.EntityFrameworkCore;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class ResourceRepository(CamarDbContext db) : IResourceRepository
{
    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task AddAsync(Resource resource, CancellationToken ct = default)
    {
        db.Resources.Add(resource);
        return db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Resource resource, CancellationToken ct = default)
    {
        db.Resources.Update(resource);
        return db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken ct = default) =>
        await db.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
}
