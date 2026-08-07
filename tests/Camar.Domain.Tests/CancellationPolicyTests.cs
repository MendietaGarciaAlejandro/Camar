using Camar.Domain.Reservations;

namespace Camar.Domain.Tests;

public class CancellationPolicyTests
{
    // La reserva es el 15/01/2026 de 10:00 a 11:00.
    private static readonly Period Reserva = new(
        new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2026, 1, 15, 11, 0, 0, TimeSpan.Zero));

    private static DateTimeOffset HorasAntes(double horas) => Reserva.Start.AddHours(-horas);

    [Theory]
    [InlineData(48, 1.0)]    // dos dias antes: entero
    [InlineData(24, 1.0)]    // justo en el limite de las 24h: entero
    [InlineData(23, 0.5)]    // ya dentro del tramo parcial
    [InlineData(3, 0.5)]     // justo en el limite de las 3h: parcial
    [InlineData(2.5, 0.0)]   // menos de 3h: no se devuelve nada
    [InlineData(0, 0.0)]     // a la hora de empezar
    public void RefundRate_DependeDeLaAntelacion(double horasAntes, decimal esperado)
    {
        var cancelledAt = HorasAntes(horasAntes);

        Assert.Equal(esperado, CancellationPolicy.RefundRate(Reserva, cancelledAt));
    }

    [Fact]
    public void RefundRate_CancelarDespuesDeEmpezar_NoDevuelveNada()
    {
        var cancelledAt = Reserva.Start.AddMinutes(15);

        Assert.Equal(0m, CancellationPolicy.RefundRate(Reserva, cancelledAt));
    }

    [Theory]
    [InlineData(48, 18.00, 18.00)]   // completo
    [InlineData(5, 18.00, 9.00)]     // mitad
    [InlineData(1, 18.00, 0.00)]     // nada
    public void CalculateRefund_AplicaElPorcentajeAlPrecio(
        double horasAntes, decimal precio, decimal esperado)
    {
        var refund = CancellationPolicy.CalculateRefund(Reserva, HorasAntes(horasAntes), precio);

        Assert.Equal(esperado, refund);
    }

    [Fact]
    public void CalculateRefund_RedondeaADosDecimales()
    {
        // 15.05 / 2 = 7.525 -> 7.53 alejandose del cero
        var refund = CancellationPolicy.CalculateRefund(Reserva, HorasAntes(5), 15.05m);

        Assert.Equal(7.53m, refund);
    }

    [Fact]
    public void Cancel_GuardaElReembolsoCalculado()
    {
        var reserva = new Reservation(
            Guid.NewGuid(), Guid.NewGuid(), Reserva, 18.00m, HorasAntes(72));

        reserva.Cancel(HorasAntes(5));

        Assert.Equal(ReservationStatus.Cancelled, reserva.Status);
        Assert.Equal(9.00m, reserva.RefundAmount);
    }

    [Fact]
    public void Cancel_SinAntelacion_NoDevuelveNada()
    {
        var reserva = new Reservation(
            Guid.NewGuid(), Guid.NewGuid(), Reserva, 18.00m, HorasAntes(72));

        reserva.Cancel(HorasAntes(1));

        Assert.Equal(0m, reserva.RefundAmount);
    }
}
