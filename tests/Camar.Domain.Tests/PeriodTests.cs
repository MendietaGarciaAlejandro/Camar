namespace Camar.Domain.Tests;

using Camar.Domain.Reservations;

public class PeriodTests
{
    // Helper: construye un Period en un día fijo, a partir de horas enteras.
    // Necesario porque [InlineData] NO admite DateTimeOffset (solo constantes).
    private static Period At(int startHour, int endHour)
    {
        var day = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        return new Period(day.AddHours(startHour), day.AddHours(endHour));
    }

    [Fact]
    public void Constructor_CuandoElFinNoEsPosteriorAlInicio_Lanza()
    {
        // Arrange
        var instante = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

        // Act + Assert: construir con end == start debe lanzar
        Assert.Throws<ArgumentException>(() => new Period(instante, instante));
    }

    [Theory]
    [InlineData(8, 9, 10, 11, false)]  // A del todo antes que B
    [InlineData(8, 9, 9, 10, false)]   // adyacentes: NO solapan (medio-abierto)
    [InlineData(8, 10, 9, 11, true)]   // solapamiento parcial
    [InlineData(8, 12, 9, 10, true)]   // B dentro de A
    [InlineData(8, 9, 8, 9, true)]     // idénticos
    public void Overlaps_DevuelveLoEsperado(int aStart, int aEnd, int bStart, int bEnd, bool esperado)
    {
        // Arrange: usa el helper At(...) para construir a y b
        var a = At(aStart, aEnd);
        var b = At(bStart, bEnd);

        // Act: llama a a.Overlaps(b)
        var resultado = a.Overlaps(b);

        // Assert: Assert.Equal(esperado, resultado)
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(8, 10, 9, 11)]   // solapan
    [InlineData(8, 9, 10, 11)]   // no solapan
    public void Overlaps_EsSimetrico(int aStart, int aEnd, int bStart, int bEnd)
    {
        // construye a y b con At(...), y asserta que ambas direcciones coinciden
        var a = At(aStart, aEnd);
        var b = At(bStart, bEnd);
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }
}
