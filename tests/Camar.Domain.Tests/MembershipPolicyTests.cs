using Camar.Domain.Members;

namespace Camar.Domain.Tests;

public class MembershipPolicyTests
{

    [Theory]
    [InlineData(MembershipPlan.Flex, 0, true)]    // reservar para hoy
    [InlineData(MembershipPlan.Flex, 7, true)]    // a 7 días
    [InlineData(MembershipPlan.Flex, 8, false)]   // a 8 días, se pasa
    [InlineData(MembershipPlan.Flex, -1, false)]   // ayer, no se puede reservar en el pasado
    [InlineData(MembershipPlan.DayPass, 0, true)]  // reservar para hoy
    [InlineData(MembershipPlan.DayPass, 1, true)]  // reservar para mañana
    [InlineData(MembershipPlan.DayPass, 2, false)] // a 2 días, se pasa
    [InlineData(MembershipPlan.DayPass, -1, false)] // ayer, no se puede reservar en el pasado
    public void PuedoReservarSegunPlan(MembershipPlan plan, int daysAhead, bool expected)
    {
        var today = new DateOnly(2026, 1, 15);
        var reservationDate = today.AddDays(daysAhead);

        var canBook = MembershipPolicy.CanBookOn(plan, today, reservationDate);

        Assert.Equal(expected, canBook);
    }
}
