namespace Camar.Domain.Members;

/// <summary>
/// Como se llama cada plan de socio en un mensaje. Igual que con los tipos de recurso, el
/// nombre del enum vale para el codigo y para el JSON, pero no para leerlo.
/// </summary>
public static class MembershipPlanNames
{
    public static string DisplayName(this MembershipPlan plan) => plan switch
    {
        // Flex se queda como esta: es como se llama el plan de cara al socio.
        MembershipPlan.Flex => "Flex",
        MembershipPlan.DayPass => "Bono de dia",
        _ => throw new ArgumentOutOfRangeException(nameof(plan)),
    };
}
