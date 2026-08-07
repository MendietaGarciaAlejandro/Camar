using Camar.Domain.Reservations;

namespace Camar.Application.Abstractions;

public interface IReservationRepository
{
    Task<bool> HasOverlapAsync(Guid resourceId, Period period, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetAllAsync(Guid? resourceId = null, CancellationToken ct = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetConfirmedInRangeAsync(
        Guid resourceId, Period range, CancellationToken ct = default);
}
