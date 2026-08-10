using Camar.Domain.Common;

namespace Camar.Domain.Members;

/// <summary>
/// Teléfono español: nueve dígitos que empiezan por 6 o 7 (móvil) u 8 o 9 (fijo).
///
/// Se admite el prefijo internacional y se descarta al guardar. Quitarlo no es ambiguo:
/// ningún número español empieza por 3, así que un 34 al principio de once dígitos solo
/// puede ser el prefijo.
/// </summary>
public readonly record struct PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string texto)
    {
        var digitos = new string(Guard.NotBlank(texto).Where(char.IsAsciiDigit).ToArray());

        var sinPrefijo = digitos switch
        {
            { Length: 13 } when digitos.StartsWith("0034") => digitos[4..],
            { Length: 11 } when digitos.StartsWith("34") => digitos[2..],
            _ => digitos,
        };

        if (sinPrefijo.Length != 9)
            throw new BusinessRuleException("El teléfono debe tener nueve dígitos.");

        // El plan de numeración reserva el 6 y el 7 para móviles y el 8 y el 9 para fijos
        // y servicios; cualquier otra cifra inicial no existe.
        if (!"6789".Contains(sinPrefijo[0]))
            throw new BusinessRuleException("Ese número de teléfono no existe en España.");

        Value = sinPrefijo;
    }

    public bool IsMobile => Value[0] is '6' or '7';

    public override string ToString() => Value;
}

/// <summary>
/// Código postal español: cinco dígitos cuyas dos primeras cifras son la provincia.
///
/// Comprobar solo que son cinco dígitos dejaría pasar «99999», que no es ningún sitio: las
/// provincias van de la 01 a la 52, siguiendo el orden alfabético fijado en el siglo XIX.
/// </summary>
public readonly record struct PostalCode
{
    public string Value { get; }

    public PostalCode(string texto)
    {
        var limpio = Identificadores.Limpiar(Guard.NotBlank(texto));

        if (limpio.Length != 5 || !limpio.All(char.IsAsciiDigit))
            throw new BusinessRuleException("El código postal debe tener cinco dígitos.");

        var provincia = int.Parse(limpio[..2]);
        if (provincia is < 1 or > 52)
            throw new BusinessRuleException("Ese código postal no corresponde a ninguna provincia.");

        Value = limpio;
    }

    /// <summary>Las dos primeras cifras, que identifican la provincia.</summary>
    public int ProvinceCode => int.Parse(Value[..2]);

    public override string ToString() => Value;
}
