using Camar.Domain.Reservations;
using Camar.Domain.Scheduling;

namespace Camar.Domain.Tests;

public class AvailabilityCalculatorTests
{
    private static readonly DateOnly Jueves = new(2026, 1, 15);
    private static readonly DateOnly Sabado = new(2026, 1, 17);
    private static readonly DateOnly Domingo = new(2026, 1, 18);

    private static Period Franja(DateOnly date, int startHour, int startMinute, int endHour, int endMinute)
    {
        var day = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return new Period(
            day.AddHours(startHour).AddMinutes(startMinute),
            day.AddHours(endHour).AddMinutes(endMinute));
    }

    [Fact]
    public void FreeBlocks_SinReservas_DevuelveTodaLaJornada()
    {
        // Jueves: 8:00 a 21:00 son 13 horas = 26 bloques de media hora
        var libres = AvailabilityCalculator.FreeBlocks(Jueves, []);

        Assert.Equal(26, libres.Count);
        Assert.Equal(new TimeSpan(8, 0, 0), libres[0].Start.TimeOfDay);
        Assert.Equal(new TimeSpan(21, 0, 0), libres[^1].End.TimeOfDay);
    }

    [Fact]
    public void FreeBlocks_ElSabadoSoloOfreceSuHorarioReducido()
    {
        // Sabado: 9:00 a 14:00 son 5 horas = 10 bloques
        var libres = AvailabilityCalculator.FreeBlocks(Sabado, []);

        Assert.Equal(10, libres.Count);
        Assert.Equal(new TimeSpan(9, 0, 0), libres[0].Start.TimeOfDay);
        Assert.Equal(new TimeSpan(14, 0, 0), libres[^1].End.TimeOfDay);
    }

    [Fact]
    public void FreeBlocks_ElDomingoNoHayNada()
    {
        var libres = AvailabilityCalculator.FreeBlocks(Domingo, []);

        Assert.Empty(libres);
    }

    [Fact]
    public void FreeBlocks_DescartaLosBloquesOcupados()
    {
        // Una reserva de 10:00 a 11:00 tapa dos bloques: 10:00-10:30 y 10:30-11:00
        var reservada = Franja(Jueves, 10, 0, 11, 0);

        var libres = AvailabilityCalculator.FreeBlocks(Jueves, [reservada]);

        Assert.Equal(24, libres.Count);
        Assert.DoesNotContain(libres, b => b.Start.TimeOfDay == new TimeSpan(10, 0, 0));
        Assert.DoesNotContain(libres, b => b.Start.TimeOfDay == new TimeSpan(10, 30, 0));
    }

    [Fact]
    public void FreeBlocks_ElBloqueAdyacenteSigueLibre()
    {
        // Con 10:00-11:00 ocupado, el de 11:00-11:30 no solapa y debe seguir disponible
        var reservada = Franja(Jueves, 10, 0, 11, 0);

        var libres = AvailabilityCalculator.FreeBlocks(Jueves, [reservada]);

        Assert.Contains(libres, b => b.Start.TimeOfDay == new TimeSpan(11, 0, 0));
        Assert.Contains(libres, b => b.Start.TimeOfDay == new TimeSpan(9, 30, 0));
    }

    [Fact]
    public void FreeBlocks_VariasReservasSeDescuentanTodas()
    {
        var manana = Franja(Jueves, 9, 0, 10, 0);   // 2 bloques
        var tarde = Franja(Jueves, 16, 0, 18, 0);   // 4 bloques

        var libres = AvailabilityCalculator.FreeBlocks(Jueves, [manana, tarde]);

        Assert.Equal(26 - 6, libres.Count);
    }
}
