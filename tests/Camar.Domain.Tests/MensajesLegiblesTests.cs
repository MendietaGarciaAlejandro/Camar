using Camar.Domain.Members;
using Camar.Domain.Resources;
using Camar.Domain.Scheduling;

namespace Camar.Domain.Tests;

/// <summary>
/// Los mensajes de error los lee un socio, no un programador. Estos tests fijan que ningun
/// nombre del enum se cuele en una frase en español.
/// </summary>
public class MensajesLegiblesTests
{
    [Theory]
    [InlineData(ResourceType.MeetingRoom, "sala de reuniones")]
    [InlineData(ResourceType.HotDesk, "mesa flexible")]
    [InlineData(ResourceType.PhoneBooth, "cabina de llamadas")]
    public void CadaTipoDeRecursoTieneNombreLegible(ResourceType tipo, string esperado)
    {
        Assert.Equal(esperado, tipo.DisplayName());
    }

    [Fact]
    public void NingunTipoSeQuedaSinNombre()
    {
        // Si alguien añade un tipo y se olvida del nombre, se entera aqui y no en
        // produccion con un mensaje a medio traducir.
        foreach (var tipo in Enum.GetValues<ResourceType>())
        {
            Assert.NotEqual(tipo.ToString(), tipo.DisplayName());
        }
    }

    [Fact]
    public void NingunPlanSeQuedaSinNombre()
    {
        Assert.Equal("Bono de dia", MembershipPlan.DayPass.DisplayName());
        // Flex si coincide con el nombre del enum: es como se llama el plan de verdad.
        Assert.Equal("Flex", MembershipPlan.Flex.DisplayName());
    }

    [Theory]
    [InlineData(30, "media hora")]
    [InlineData(60, "una hora")]
    [InlineData(90, "hora y media")]
    [InlineData(45, "45 minutos")]
    [InlineData(240, "4 horas")]
    [InlineData(150, "2 horas y 30 minutos")]
    public void LasDuracionesSeDicenComoLasDiriaAlguien(int minutos, string esperado)
    {
        Assert.Equal(esperado, DurationText.Describe(TimeSpan.FromMinutes(minutos)));
    }

    [Fact]
    public void ElRangoDeLasMesasFlexiblesNoRepiteLaUnidad()
    {
        var (min, max) = BookingRules.DurationLimits(ResourceType.HotDesk);

        Assert.Equal("entre 4 y 13 horas", DurationText.DescribeRange(min, max));
    }

    [Fact]
    public void ElRangoDeLasSalasMezclaMediaHoraConHoras()
    {
        var (min, max) = BookingRules.DurationLimits(ResourceType.MeetingRoom);

        Assert.Equal("entre media hora y 4 horas", DurationText.DescribeRange(min, max));
    }

    [Fact]
    public void ElRangoDeLasCabinasSeLeeEntero()
    {
        var (min, max) = BookingRules.DurationLimits(ResourceType.PhoneBooth);

        // Aqui no se junta la unidad: "entre 1 y 1 horas" no lo dice nadie.
        Assert.Equal("entre media hora y una hora", DurationText.DescribeRange(min, max));
    }
}
