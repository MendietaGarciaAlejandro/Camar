using Camar.Application.Abstractions;
using Camar.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Camar.Infrastructure.Persistence.Repositories;

public sealed class BlockedDayRepository(CamarDbContext db) : IBlockedDayRepository
{
    public Task<BlockedDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default) =>
        db.BlockedDays.FirstOrDefaultAsync(b => b.Date == date, ct);

    public async Task<IReadOnlyList<BlockedDay>> GetAllAsync(CancellationToken ct = default) =>
        await db.BlockedDays.OrderBy(b => b.Date).ToListAsync(ct);

    public Task AddAsync(BlockedDay blockedDay, CancellationToken ct = default)
    {
        db.BlockedDays.Add(blockedDay);
        return db.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await db.BlockedDays.Where(b => b.Id == id).ExecuteDeleteAsync(ct);

        return deleted > 0;
    }
}
