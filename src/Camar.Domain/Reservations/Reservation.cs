using Camar.Domain.Common;

namespace Camar.Domain.Reservations
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid ResourceId { get; private set; }
        public Guid UserId { get; private set; }
        public Period Period { get; private set; }
        public ReservationStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public decimal Price { get; private set; }
        public decimal? RefundAmount { get; private set; }

        public Reservation(Guid resourceId, Guid userId, Period period, decimal price, DateTimeOffset createdAt)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(price, nameof(price));
            ResourceId = Guard.NotEmpty(resourceId);
            UserId = Guard.NotEmpty(userId);
            Period = period;
            Price = price;

            Id = Guid.CreateVersion7();
            Status = ReservationStatus.Confirmed;
            CreatedAt = createdAt;
            CancelledAt = null;
        }

        /// <summary>
        /// Cancela la reserva y fija el reembolso segun la antelacion del aviso.
        /// El importe lo decide la politica: no se puede pasar por fuera.
        /// </summary>
        public void Cancel(DateTimeOffset cuando)
        {
            EnsureConfirmed();

            Status = ReservationStatus.Cancelled;
            CancelledAt = cuando;
            RefundAmount = CancellationPolicy.CalculateRefund(Period, cuando, Price);
        }

        public void Complete()
        {
            EnsureConfirmed();
            Status = ReservationStatus.Completed;
        }

        public void MarkNoShow()
        {
            EnsureConfirmed();
            Status = ReservationStatus.NoShow;
        }

        private void EnsureConfirmed()
        {
            if (Status != ReservationStatus.Confirmed)
                throw new InvalidOperationException(
                    $"No se puede operar sobre una reserva en estado {Status}.");
        }
    }
}
