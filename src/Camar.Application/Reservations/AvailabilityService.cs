using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Reservations;
using Camar.Domain.Scheduling;

namespace Camar.Application.Reservations;

public class AvailabilityService(
    IReservationRepository reservations,
    IResourceRepository resources,
    IBlockedDayRepository blockedDays)
{
    /// <summary>
    /// Bloques de media hora libres de un recurso en un dia. Vacio si esta cerrado.
    /// </summary>
    public async Task<IReadOnlyList<Period>> GetFreeBlocksAsync(
        Guid resourceId,
        DateOnly date,
        CancellationToken ct = default)
    {
        var resource = await resources.GetByIdAsync(resourceId, ct)
            ?? throw new NotFoundException($"No existe el recurso {resourceId}.");

        if (!resource.IsActive)
            throw new BusinessRuleException($"El recurso '{resource.Name}' no esta disponible.");

        if (OpeningHoursPolicy.GetHours(date.DayOfWeek) is not (var opens, var closes))
            return [];

        if (await blockedDays.GetByDateAsync(date, ct) is not null)
            return [];

        // Solo se traen las reservas que tocan ese dia, no todo el historico del recurso.
        var jornada = new Period(
            new DateTimeOffset(date.ToDateTime(opens), TimeSpan.Zero),
            new DateTimeOffset(date.ToDateTime(closes), TimeSpan.Zero));

        var ocupadas = await reservations.GetConfirmedInRangeAsync(resourceId, jornada, ct);

        return AvailabilityCalculator.FreeBlocks(date, ocupadas.Select(r => r.Period));
    }
}
