using Camar.Domain.Reservations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Camar.Infrastructure.Persistence.Converters;

/// <summary>
/// Traduce el value object Period a un tstzrange de Postgres.
/// El rango se construye [inicio, fin) igual que Period, para que la constraint
/// de exclusion no vea como solapadas dos reservas consecutivas.
/// </summary>
public sealed class PeriodConverter : ValueConverter<Period, NpgsqlRange<DateTimeOffset>>
{
    public PeriodConverter()
        : base(
            period => new NpgsqlRange<DateTimeOffset>(
                period.Start, lowerBoundIsInclusive: true,
                period.End, upperBoundIsInclusive: false),
            range => new Period(range.LowerBound, range.UpperBound))
    {
    }
}
