namespace Camar.Domain.Reservations;

/// <summary>
/// Cuanto se devuelve al cancelar, segun la antelacion con la que se avisa.
/// Cuanto mas tarde se cancela, menos margen hay para revender el hueco.
/// </summary>
public static class CancellationPolicy
{
    public static readonly TimeSpan FullRefundThreshold = TimeSpan.FromHours(24);
    public static readonly TimeSpan PartialRefundThreshold = TimeSpan.FromHours(3);

    private const decimal PartialRate = 0.5m;

    public static decimal RefundRate(Period period, DateTimeOffset cancelledAt)
    {
        var advance = period.Start - cancelledAt;

        if (advance >= FullRefundThreshold) return 1m;
        if (advance >= PartialRefundThreshold) return PartialRate;

        return 0m;
    }

    public static decimal CalculateRefund(Period period, DateTimeOffset cancelledAt, decimal price)
    {
        var refund = price * RefundRate(period, cancelledAt);

        // Dinero: se redondea a dos decimales alejandose del cero, como en un recibo.
        return Math.Round(refund, 2, MidpointRounding.AwayFromZero);
    }
}
