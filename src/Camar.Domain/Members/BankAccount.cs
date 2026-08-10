using Camar.Domain.Common;

namespace Camar.Domain.Members;

/// <summary>
/// IBAN para domiciliar las reservas.
///
/// Es opcional: solo hace falta si el socio quiere que se le cobre a la cuenta. Quien paga
/// cada reserva al momento no tiene por qué darlo.
///
/// El control se comprueba moviendo los cuatro primeros caracteres al final, cambiando
/// cada letra por su posición en el alfabeto más nueve (A vale 10, Z vale 35) y viendo que
/// el número resultante da resto 1 al dividir entre 97. Ese número tiene más de veinte
/// cifras y no cabe en un long, así que el resto se arrastra cifra a cifra, igual que se
/// divide a mano.
/// </summary>
public readonly record struct BankAccount
{
    public string Value { get; }

    public BankAccount(string texto)
    {
        var limpio = Identificadores.Limpiar(Guard.NotBlank(texto));

        if (limpio.Length is < 15 or > 34)
            throw new BusinessRuleException("El IBAN no tiene una longitud válida.");

        if (!char.IsAsciiLetterUpper(limpio[0]) || !char.IsAsciiLetterUpper(limpio[1])
            || !char.IsAsciiDigit(limpio[2]) || !char.IsAsciiDigit(limpio[3]))
        {
            throw new BusinessRuleException("El IBAN debe empezar por dos letras de país y dos dígitos.");
        }

        if (Resto97(limpio) != 1)
            throw new BusinessRuleException("El IBAN no es válido: los dígitos de control no cuadran.");

        Value = limpio;
    }

    /// <summary>Las dos letras del país.</summary>
    public string Country => Value[..2];

    /// <summary>Separado en grupos de cuatro, como se escribe en papel.</summary>
    public string Formatted() => string.Join(' ', Value.Chunk(4).Select(c => new string(c)));

    public override string ToString() => Value;

    private static int Resto97(string iban)
    {
        var resto = 0;

        // Se empieza por el quinto carácter y se vuelve al principio al final: es la
        // rotación que pide la norma, hecha sin copiar la cadena.
        for (var posicion = 0; posicion < iban.Length; posicion++)
        {
            var caracter = iban[(posicion + 4) % iban.Length];

            resto = char.IsAsciiDigit(caracter)
                ? (resto * 10 + (caracter - '0')) % 97
                // Una letra son dos dígitos, así que se multiplica por cien y no por diez.
                : (resto * 100 + (caracter - 'A' + 10)) % 97;
        }

        return resto;
    }
}
