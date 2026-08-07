using Camar.Domain.Reservations;
using Camar.Domain.Scheduling;

namespace Camar.Domain.Tests;

public class OpeningHoursPolicyTests
{
    // 15/01/2026 es jueves; 17/01 sabado; 18/01 domingo.
    private const int Jueves = 15;
    private const int Sabado = 17;
    private const int Domingo = 18;

    private static Period At(int day, int startHour, int startMinute, int endHour, int endMinute)
    {
        var date = new DateTimeOffset(2026, 1, day, 0, 0, 0, TimeSpan.Zero);
        return new Period(
            date.AddHours(startHour).AddMinutes(startMinute),
            date.AddHours(endHour).AddMinutes(endMinute));
    }

    [Fact]
    public void GetHours_ElDomingoEstaCerrado()
    {
        Assert.Null(OpeningHoursPolicy.GetHours(DayOfWeek.Sunday));
    }

    [Fact]
    public void GetHours_ElSabadoTieneHorarioReducido()
    {
        Assert.Equal((new TimeOnly(9, 0), new TimeOnly(14, 0)),
            OpeningHoursPolicy.GetHours(DayOfWeek.Saturday));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public void GetHours_EntreSemanaAbreDeOchoANueveDeLaNoche(DayOfWeek day)
    {
        Assert.Equal((new TimeOnly(8, 0), new TimeOnly(21, 0)),
            OpeningHoursPolicy.GetHours(day));
    }

    [Theory]
    [InlineData(10, 0, 11, 0, true)]   // en pleno horario
    [InlineData(8, 0, 9, 0, true)]     // justo a la apertura
    [InlineData(20, 0, 21, 0, true)]   // justo hasta el cierre
    [InlineData(7, 0, 9, 0, false)]    // empieza antes de abrir
    [InlineData(20, 0, 22, 0, false)]  // termina despues de cerrar
    [InlineData(6, 0, 7, 0, false)]    // entera fuera de horario
    public void IsWithinOpeningHours_EntreSemana(
        int startHour, int startMinute, int endHour, int endMinute, bool esperado)
    {
        var period = At(Jueves, startHour, startMinute, endHour, endMinute);

        Assert.Equal(esperado, OpeningHoursPolicy.IsWithinOpeningHours(period));
    }

    [Theory]
    [InlineData(10, 0, 11, 0, true)]   // dentro del horario de sabado
    [InlineData(9, 0, 14, 0, true)]    // la franja completa
    [InlineData(8, 0, 9, 0, false)]    // el sabado no abre hasta las 9
    [InlineData(13, 0, 15, 0, false)]  // cierra a las 14
    public void IsWithinOpeningHours_ElSabado(
        int startHour, int startMinute, int endHour, int endMinute, bool esperado)
    {
        var period = At(Sabado, startHour, startMinute, endHour, endMinute);

        Assert.Equal(esperado, OpeningHoursPolicy.IsWithinOpeningHours(period));
    }

    [Fact]
    public void IsWithinOpeningHours_ElDomingoNuncaSePuedeReservar()
    {
        var period = At(Domingo, 10, 0, 11, 0);

        Assert.False(OpeningHoursPolicy.IsWithinOpeningHours(period));
    }

    [Fact]
    public void IsWithinOpeningHours_NoSePuedeCruzarLaMedianoche()
    {
        var jueves = new DateTimeOffset(2026, 1, Jueves, 0, 0, 0, TimeSpan.Zero);
        var period = new Period(jueves.AddHours(23), jueves.AddHours(25));

        Assert.False(OpeningHoursPolicy.IsWithinOpeningHours(period));
    }
}
