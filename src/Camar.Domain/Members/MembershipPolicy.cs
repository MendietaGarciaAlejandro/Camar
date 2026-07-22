namespace Camar.Domain.Members
{
    public static class MembershipPolicy
    {
        // 1) Máximo de días de antelación según el plan
        public static int MaxAdvanceDays(MembershipPlan plan) => plan switch
        {
            MembershipPlan.Flex => 7,
            MembershipPlan.DayPass => 1,   // hoy (0) + mañana (1)
            _ => throw new ArgumentOutOfRangeException(nameof(plan)),
        };

        // 2) ¿Puede reservar para esa fecha, dado el "hoy"?
        public static bool CanBookOn(MembershipPlan plan, DateOnly today, DateOnly reservationDate)
        {
            return reservationDate >= today
                && reservationDate <= today.AddDays(MaxAdvanceDays(plan));
        }
    }
}
