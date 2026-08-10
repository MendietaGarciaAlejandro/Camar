namespace Camar.Domain.Members;

/// <summary>
/// Utilidades compartidas por los identificadores fiscales y de contacto.
/// </summary>
internal static class Identificadores
{
    /// <summary>
    /// Deja solo letras y dígitos, en mayúsculas.
    ///
    /// La gente escribe «12345678-Z» o «ES91 2100 0418 4502 0005 1332», y son la misma
    /// entrada. Se comparan los rangos a mano en lugar de usar char.IsLetterOrDigit, que
    /// aceptaría la «Ñ» o una vocal acentuada: en un identificador oficial eso no es una
    /// letra válida sino una errata.
    /// </summary>
    internal static string Limpiar(string texto)
    {
        var limpio = new System.Text.StringBuilder(texto.Length);

        foreach (var caracter in texto.ToUpperInvariant())
        {
            if (caracter is >= '0' and <= '9' or >= 'A' and <= 'Z')
                limpio.Append(caracter);
        }

        return limpio.ToString();
    }

    /// <summary>
    /// Orden oficial de las letras de control del DNI. No es alfabético: es el que fija la
    /// normativa. Faltan la I, la Ñ, la O y la U para que no se confundan con el 1 y el 0.
    /// </summary>
    private const string LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE";

    internal static char LetraDeControl(int numero) => LetrasDni[numero % 23];
}
