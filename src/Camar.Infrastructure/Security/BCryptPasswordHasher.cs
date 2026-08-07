using Camar.Application.Abstractions;

namespace Camar.Infrastructure.Security;

/// <summary>
/// BCrypt genera y comprueba la sal por su cuenta: el hash resultante ya la lleva dentro.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    // Coste 12: unos cientos de ms por hash, suficiente para frenar fuerza bruta.
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash con formato invalido (por ejemplo el placeholder del seed).
            return false;
        }
    }
}
