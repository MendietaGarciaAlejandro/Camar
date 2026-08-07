using Camar.Domain.Reservations;
using Camar.Domain.Resources;

namespace Camar.Domain.Tests
{
    public class ReservationTests
    {
        private static Reservation NuevaReserva()
        {
            var dia = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var periodo = new Period(dia.AddHours(10), dia.AddHours(11));
            return new Reservation(Guid.NewGuid(), Guid.NewGuid(), periodo, 12.00m, dia);
        }

        [Fact]
        public void Constructor_PrecioNegativo_Lanza()
        {
            var dia = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var periodo = new Period(dia.AddHours(10), dia.AddHours(11));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Reservation(Guid.NewGuid(), Guid.NewGuid(), periodo, -1m, dia));
        }

        [Fact]
        public void Confirmed_ReservaNaceConfirmada()
        {
            var reserva = NuevaReserva();
            Assert.Equal(ReservationStatus.Confirmed, reserva.Status);
        }

        [Fact]
        public void Excepcion_GuidResourceVacio()
        {
            var reservaRecursoGuidVacio = () => new Reservation(Guid.Empty, Guid.NewGuid(), new Period(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1)), 12.00m, DateTimeOffset.Now);
            Assert.Throws<ArgumentException>(reservaRecursoGuidVacio);
        }

        [Fact]
        public void Excepcion_GuidUserVacio()
        {
            var reservaUsuarioGuidVacio = () => new Reservation(Guid.NewGuid(), Guid.Empty, new Period(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1)), 12.00m, DateTimeOffset.Now);
            Assert.Throws<ArgumentException>(reservaUsuarioGuidVacio);
        }

        [Fact]
        public void Cancel_StatusEsCancelled()
        {
            var reserva = NuevaReserva();
            var cuando = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
            reserva.Cancel(cuando);
            Assert.Equal(ReservationStatus.Cancelled, reserva.Status);
            Assert.Equal(cuando, reserva.CancelledAt);
        }

        [Fact]
        public void Complete_StatusEsCompleted()
        {
            var reserva = NuevaReserva();
            reserva.Complete();
            Assert.Equal(ReservationStatus.Completed, reserva.Status);
        }

        [Fact]
        public void MarkNoShow_StatusEsNoShow()
        {
            var reserva = NuevaReserva();
            reserva.MarkNoShow();
            Assert.Equal(ReservationStatus.NoShow, reserva.Status);
        }

        [Fact]
        public void Cancel_CancelarDosVecesLanzaInvalidOperationException()
        {
            var reserva = NuevaReserva();
            reserva.Cancel(DateTimeOffset.Now);
            Assert.Throws<InvalidOperationException>(() => reserva.Cancel(DateTimeOffset.Now));
        }

        [Fact]
        public void Cancel_LuegoComplete_LanzaInvalidOperationException()
        {
            var reserva = NuevaReserva();
            reserva.Complete();
            Assert.Throws<InvalidOperationException>(() => reserva.Cancel(DateTimeOffset.Now));
        }
    }
}