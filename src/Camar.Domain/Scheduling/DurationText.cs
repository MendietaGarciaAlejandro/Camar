namespace Camar.Domain.Scheduling;

/// <summary>
/// Duraciones escritas como las diria una persona.
///
/// Decir "entre 240 y 780 minutos" obliga al socio a dividir entre sesenta para entender
/// que le estan diciendo cuatro horas y trece.
/// </summary>
public static class DurationText
{
    public static string Describe(TimeSpan duration)
    {
        var minutos = (int)duration.TotalMinutes;

        if (minutos == 30) return "media hora";
        if (minutos == 60) return "una hora";
        if (minutos == 90) return "hora y media";

        var horas = minutos / 60;
        var resto = minutos % 60;

        if (horas == 0) return $"{resto} minutos";
        if (resto == 0) return $"{horas} horas";

        return $"{horas} horas y {resto} minutos";
    }

    /// <summary>
    /// El rango entero. Cuando los dos extremos son horas justas se dice la unidad una
    /// sola vez: "entre 4 y 13 horas" en vez de "entre 4 horas y 13 horas".
    /// </summary>
    public static string DescribeRange(TimeSpan min, TimeSpan max)
    {
        var minutosMin = (int)min.TotalMinutes;
        var minutosMax = (int)max.TotalMinutes;

        // Con una sola hora la frase seria "entre 1 y 4 horas", que se lee raro; en ese
        // caso es mejor dejar que cada extremo se describa por su cuenta.
        if (minutosMin % 60 == 0 && minutosMax % 60 == 0 && minutosMin >= 120)
            return $"entre {minutosMin / 60} y {minutosMax / 60} horas";

        return $"entre {Describe(min)} y {Describe(max)}";
    }
}
