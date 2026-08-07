using Camar.Domain.Reservations;
using Camar.Domain.Resources;

namespace Camar.Domain.Pricing;

public static class PricingPolicy
{
    private const int BlockMinutes = 30;
    private const decimal PeakMultiplier = 1.5m;

    private static decimal BaseRatePerBlock(ResourceType type) => type switch
    {
        ResourceType.MeetingRoom => 6.00m,
        ResourceType.HotDesk => 3.00m,
        ResourceType.PhoneBooth => 2.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    private static bool IsPeak(DateTimeOffset when) => when.Hour >= 9 && when.Hour < 18;

    public static decimal CalculatePrice(ResourceType type, Period period)
    {
        var baseRate = BaseRatePerBlock(type);
        var total = 0m;
        for (var bloque = period.Start; bloque < period.End; bloque = bloque.AddMinutes(BlockMinutes))
        {
            total += IsPeak(bloque) ? baseRate * PeakMultiplier : baseRate;
        }
        return total;
    }
}