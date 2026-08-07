using Camar.Domain.Reservations;

namespace Camar.Api.Contracts;

/// <summary>Datos para crear una reserva. El usuario sale del token.</summary>
public sealed record CreateReservationRequest(
    Guid ResourceId,
    DateTimeOffset Start,
    DateTimeOffset End);

public sealed record ReservationResponse(
    Guid Id,
    Guid ResourceId,
    Guid UserId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Status,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    decimal? RefundAmount)
{
    public static ReservationResponse From(Reservation reservation) => new(
        reservation.Id,
        reservation.ResourceId,
        reservation.UserId,
        reservation.Period.Start,
        reservation.Period.End,
        reservation.Status.ToString(),
        reservation.Price,
        reservation.CreatedAt,
        reservation.CancelledAt,
        reservation.RefundAmount);
}
