using Camar.Domain.Reservations;

namespace Camar.Api.Contracts;

/// <summary>
/// Datos para crear una reserva. UserId viaja en el cuerpo de forma temporal:
/// cuando exista autenticacion saldra del token y dejara de pedirse aqui.
/// </summary>
public sealed record CreateReservationRequest(
    Guid ResourceId,
    Guid UserId,
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
    DateTimeOffset? CancelledAt)
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
        reservation.CancelledAt);
}
