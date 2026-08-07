using Camar.Domain.Reservations;

namespace Camar.Domain.Scheduling;

/// <summary>
/// Huecos libres de un recurso en un dia concreto, en bloques de media hora.
/// Se asume que las horas van en la zona del coworking (ver nota de zonas horarias).
/// </summary>
public static class AvailabilityCalculator
{
    public static IReadOnlyList<Period> FreeBlocks(DateOnly date, IEnumerable<Period> taken)
    {
        if (OpeningHoursPolicy.GetHours(date.DayOfWeek) is not (var opens, var closes))
            return [];

        var ocupados = taken as IList<Period> ?? taken.ToList();
        var block = TimeSpan.FromMinutes(BookingRules.BlockMinutes);

        var dayStart = new DateTimeOffset(date.ToDateTime(opens), TimeSpan.Zero);
        var dayEnd = new DateTimeOffset(date.ToDateTime(closes), TimeSpan.Zero);

        var libres = new List<Period>();

        for (var start = dayStart; start + block <= dayEnd; start += block)
        {
            var candidato = new Period(start, start + block);

            if (!ocupados.Any(o => o.Overlaps(candidato)))
                libres.Add(candidato);
        }

        return libres;
    }
}
