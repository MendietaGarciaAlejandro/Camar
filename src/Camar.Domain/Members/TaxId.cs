using Camar.Domain.Common;

namespace Camar.Domain.Members;

/// <summary>Qué clase de documento fiscal es.</summary>
public enum TaxIdKind
{
    /// <summary>DNI con letra, de persona física.</summary>
    Nif = 1,

    /// <summary>Documento de extranjero.</summary>
    Nie = 2,

    /// <summary>Identificador fiscal de una organización.</summary>
    Cif = 3,
}

/// <summary>
/// Documento fiscal del socio: NIF, NIE o CIF.
///
/// Hace falta para poder emitir facturas, así que se pide al darse de alta. Se valida en el
/// servidor aunque el cliente ya lo haga: la validación del cliente es para que la persona
/// no se lleve un chasco al enviar el formulario, pero no se puede confiar en ella porque
/// cualquiera puede llamar a la API sin pasar por la aplicación.
/// </summary>
public readonly record struct TaxId
{
    public string Value { get; }
    public TaxIdKind Kind { get; }

    public TaxId(string texto)
    {
        var limpio = Identificadores.Limpiar(Guard.NotBlank(texto));

        if (limpio.Length != 9)
            throw new BusinessRuleException("El documento fiscal debe tener nueve caracteres.");

        Kind = Reconocer(limpio);
        Value = limpio;
    }

    public override string ToString() => Value;

    private static TaxIdKind Reconocer(string limpio)
    {
        if (EsNif(limpio)) return TaxIdKind.Nif;
        if (EsNie(limpio)) return TaxIdKind.Nie;
        if (EsCif(limpio)) return TaxIdKind.Cif;

        throw new BusinessRuleException("El documento fiscal no es un NIF, NIE ni CIF válido.");
    }

    /// <summary>Ocho dígitos y la letra que sale de dividir el número entre 23.</summary>
    private static bool EsNif(string limpio)
    {
        var numero = limpio[..8];

        return numero.All(char.IsAsciiDigit)
            && char.IsAsciiLetterUpper(limpio[8])
            && limpio[8] == Identificadores.LetraDeControl(int.Parse(numero));
    }

    /// <summary>
    /// Igual que el NIF, pero la letra inicial pasa a ser el primer dígito: X vale 0,
    /// Y vale 1 y Z vale 2, según la tanda en la que se emitió.
    /// </summary>
    private static bool EsNie(string limpio)
    {
        var posicion = "XYZ".IndexOf(limpio[0]);
        var cuerpo = limpio[1..8];

        if (posicion < 0 || !cuerpo.All(char.IsAsciiDigit) || !char.IsAsciiLetterUpper(limpio[8]))
            return false;

        return limpio[8] == Identificadores.LetraDeControl(int.Parse($"{posicion}{cuerpo}"));
    }

    /// <summary>
    /// Letra de tipo de entidad, siete dígitos y un control calculado con el algoritmo de
    /// Luhn, el mismo de las tarjetas de crédito.
    ///
    /// Lo peculiar es que ese control se escribe como número o como letra según el tipo:
    /// las sociedades usan dígito, los organismos públicos y las entidades extranjeras usan
    /// letra, y un grupo intermedio admite las dos formas.
    /// </summary>
    private static bool EsCif(string limpio)
    {
        const string tipos = "ABCDEFGHJNPQRSUVW";
        const string letrasControl = "JABCDEFGHI";
        const string soloDigito = "ABEH";
        const string soloLetra = "KPQRSNW";

        var tipo = limpio[0];
        var cuerpo = limpio[1..8];

        if (!tipos.Contains(tipo) || !cuerpo.All(char.IsAsciiDigit)) return false;

        var esperado = DigitoDeControlCif(cuerpo);
        var comoDigito = (char)('0' + esperado);
        var comoLetra = letrasControl[esperado];
        var control = limpio[8];

        if (soloDigito.Contains(tipo)) return control == comoDigito;
        if (soloLetra.Contains(tipo)) return control == comoLetra;

        return control == comoDigito || control == comoLetra;
    }

    private static int DigitoDeControlCif(string cuerpo)
    {
        var total = 0;

        for (var indice = 0; indice < cuerpo.Length; indice++)
        {
            var cifra = cuerpo[indice] - '0';

            if (indice % 2 == 0)
            {
                // Posiciones impares contando desde 1: se duplican y se suman sus dígitos.
                var doble = cifra * 2;
                total += doble / 10 + doble % 10;
            }
            else
            {
                total += cifra;
            }
        }

        // Lo que falta para la siguiente decena; si ya es exacta, el control es 0.
        return (10 - total % 10) % 10;
    }
}
