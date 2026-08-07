using Camar.Domain.Reservations;

namespace Camar.Domain.Scheduling;

/// <summary>
/// Horario de apertura de Camar Coworking.
///
/// LIMITACION CONOCIDA: se da por hecho que el offset del DateTimeOffset recibido ya
/// corresponde a la hora local del coworking, en vez de convertir el instante con la
/// zona horaria real (Europe/Madrid). Con un cliente en otro huso, o cruzando el cambio
/// de hora, las comprobaciones de franja no serian correctas. Se resolveria pasando la
/// zona como TimeZoneInfo y convirtiendo el instante antes de mirar hora y dia.
/// </summary>
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
