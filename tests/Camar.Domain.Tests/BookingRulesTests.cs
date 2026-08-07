using Camar.Domain.Reservations;
using Camar.Domain.Resources;
using Camar.Domain.Scheduling;

namespace Camar.Domain.Tests;

public class BookingRulesTests
{
    private static readonly DateTimeOffset Dia = new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

    private static Period Desde(int startMinutes, int durationMinutes) =>
        new(Dia.AddMinutes(startMinutes), Dia.AddMinutes(startMinutes + durationMinutes));

    [Theory]
    [InlineData(10 * 60, 60, true)]        // 10:00 -> 11:00
    [InlineData(10 * 60 + 30, 60, true)]   // 10:30 -> 11:30
    [InlineData(10 * 60 + 15, 60, false)]  // empieza a y cuarto
    [InlineData(10 * 60, 45, false)]       // termina a menos cuarto
    [InlineData(10 * 60 + 10, 20, false)]  // ni inicio ni fin alineados
    public void IsAligned_ExigeBloquesDeMediaHora(int startMinutes, int durationMinutes, bool esperado)
    {
        var period = Desde(startMinutes, durationMinutes);

        Assert.Equal(esperado, BookingRules.IsAligned(period));
    }

    [Fact]
    public void IsAligned_LosSegundosSueltosRompenLaAlineacion()
    {
        var period = new Period(Dia.AddHours(10).AddSeconds(30), Dia.AddHours(11));

        Assert.False(BookingRules.IsAligned(period));
    }

    [Theory]
    [InlineData(ResourceType.MeetingRoom, 30, true)]    // el minimo
    [InlineData(ResourceType.MeetingRoom, 240, true)]   // el maximo: 4h
    [InlineData(ResourceType.MeetingRoom, 270, false)]  // 4h30, se pasa
    [InlineData(ResourceType.PhoneBooth, 30, true)]
    [InlineData(ResourceType.PhoneBooth, 60, true)]     // el maximo: 1h
    [InlineData(ResourceType.PhoneBooth, 90, false)]    // la cabina es de uso expres
    [InlineData(ResourceType.HotDesk, 240, true)]       // el minimo: media jornada
    [InlineData(ResourceType.HotDesk, 120, false)]      // 2h, no se alquila por ratos
    [InlineData(ResourceType.HotDesk, 780, true)]       // el maximo: 13h
    public void IsValidDuration_CadaTipoTieneSusLimites(
        ResourceType type, int durationMinutes, bool esperado)
    {
        var period = Desde(8 * 60, durationMinutes);

        Assert.Equal(esperado, BookingRules.IsValidDuration(type, period));
    }

    [Fact]
    public void DurationLimits_TipoDesconocido_Lanza()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BookingRules.DurationLimits((ResourceType)99));
    }
}
