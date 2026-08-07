using Camar.Domain.Pricing;
using Camar.Domain.Reservations;
using Camar.Domain.Resources;

namespace Camar.Domain.Tests;

public class PricingPolicyTests
{
    // Construye un periodo del 15/01/2026 a partir de horas y minutos.
    private static Period At(int startHour, int startMinute, int endHour, int endMinute)
    {
        var day = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new Period(
            day.AddHours(startHour).AddMinutes(startMinute),
            day.AddHours(endHour).AddMinutes(endMinute));
    }

    [Fact]
    public void CalculatePrice_UnaHoraEnValle_CobraLaTarifaBase()
    {
        // 8:00-9:00 -> 2 bloques valle x 6.00
        var precio = PricingPolicy.CalculatePrice(ResourceType.MeetingRoom, At(8, 0, 9, 0));

        Assert.Equal(12.00m, precio);
    }

    [Fact]
    public void CalculatePrice_UnaHoraEnPunta_AplicaElRecargo()
    {
        // 10:00-11:00 -> 2 bloques punta x 6.00 x 1.5
        var precio = PricingPolicy.CalculatePrice(ResourceType.MeetingRoom, At(10, 0, 11, 0));

        Assert.Equal(18.00m, precio);
    }

    [Fact]
    public void CalculatePrice_CruzandoElCambioDeFranja_TarificaCadaBloqueEnSuFranja()
    {
        // 17:00-19:00 -> 17:00 y 17:30 en punta (9.00 c/u), 18:00 y 18:30 en valle (6.00 c/u)
        var precio = PricingPolicy.CalculatePrice(ResourceType.MeetingRoom, At(17, 0, 19, 0));

        Assert.Equal(30.00m, precio);
    }

    [Fact]
    public void CalculatePrice_MediaHora_CobraUnSoloBloque()
    {
        // 8:00-8:30 -> 1 bloque valle
        var precio = PricingPolicy.CalculatePrice(ResourceType.MeetingRoom, At(8, 0, 8, 30));

        Assert.Equal(6.00m, precio);
    }

    [Theory]
    [InlineData(ResourceType.MeetingRoom, 12.00)]
    [InlineData(ResourceType.HotDesk, 6.00)]
    [InlineData(ResourceType.PhoneBooth, 4.00)]
    public void CalculatePrice_CadaTipoTieneSuTarifa(ResourceType type, decimal esperado)
    {
        // una hora en valle: 2 bloques a tarifa base de cada tipo
        var precio = PricingPolicy.CalculatePrice(type, At(8, 0, 9, 0));

        Assert.Equal(esperado, precio);
    }

    [Fact]
    public void CalculatePrice_ElUltimoBloqueDePuntaEsElDeLas1730()
    {
        // 18:00 ya es valle: 18:00-18:30 no lleva recargo
        var precio = PricingPolicy.CalculatePrice(ResourceType.MeetingRoom, At(18, 0, 18, 30));

        Assert.Equal(6.00m, precio);
    }
}
