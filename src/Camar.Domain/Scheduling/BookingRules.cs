using Camar.Domain.Reservations;
using Camar.Domain.Resources;

namespace Camar.Domain.Scheduling;

public static class BookingRules
{
    public const int BlockMinutes = 30;

    public static (TimeSpan Min, TimeSpan Max) DurationLimits(ResourceType type) => type switch
    {
        ResourceType.MeetingRoom => (TimeSpan.FromMinutes(30), TimeSpan.FromHours(4)),
        ResourceType.PhoneBooth => (TimeSpan.FromMinutes(30), TimeSpan.FromHours(1)),
        ResourceType.HotDesk => (TimeSpan.FromHours(4), TimeSpan.FromHours(13)),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    // Inicio y fin deben caer en :00 o :30 exactos
    private static bool IsAligned(DateTimeOffset instant) =>
    instant.TimeOfDay.Ticks % (BlockMinutes * TimeSpan.TicksPerMinute) == 0;

    public static bool IsAligned(Period period) =>
        IsAligned(period.Start) && IsAligned(period.End);

    public static bool IsValidDuration(ResourceType type, Period period)
    {
        var (min, max) = DurationLimits(type);
        return period.Duration >= min && period.Duration <= max;
    }
}