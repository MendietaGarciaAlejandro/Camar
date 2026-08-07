using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Camar.Api;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id del usuario autenticado. Sale del token, nunca de la peticion:
    /// asi nadie puede operar en nombre de otro cambiando un parametro.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("El token no trae un identificador de usuario valido.");
    }
}
