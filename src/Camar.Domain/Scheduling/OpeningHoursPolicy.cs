using Camar.Domain.Reservations;

namespace Camar.Domain.Scheduling;

public static class OpeningHoursPolicy
{
    // Devuelve el horario de ese día, o null si está cerrado.
    public static (TimeOnly Opens, TimeOnly Closes)? GetHours(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => null,
        DayOfWeek.Saturday => (new TimeOnly(9, 0), new TimeOnly(14, 0)),
        _ => (new TimeOnly(8, 0), new TimeOnly(21, 0)),
    };

    // ¿Cabe el periodo entero dentro del horario de apertura?
    public static bool IsWithinOpeningHours(Period period)
    {
        // Asumimos reservas dentro del mismo día: no contemplamos cruzar medianoche.
        if (period.Start.Date != period.End.Date)
            return false;

        if (GetHours(period.Start.DayOfWeek) is not (var opens, var closes))
            return false;

        var startTime = TimeOnly.FromTimeSpan(period.Start.TimeOfDay);
        var endTime = TimeOnly.FromTimeSpan(period.End.TimeOfDay);

        return startTime >= opens && endTime <= closes;
    }
}