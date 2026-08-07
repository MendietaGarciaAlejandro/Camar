using Camar.Domain.Scheduling;

namespace Camar.Application.Abstractions;

public interface IBlockedDayRepository
{
    Task<BlockedDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<BlockedDay>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(BlockedDay blockedDay, CancellationToken ct = default);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
