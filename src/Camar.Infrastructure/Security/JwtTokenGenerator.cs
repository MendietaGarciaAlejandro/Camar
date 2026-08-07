using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Camar.Application.Abstractions;
using Camar.Domain.Members;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Camar.Infrastructure.Security;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider clock) : ITokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) Generate(User user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.LifetimeMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            // El plan viaja en el token para no consultarlo en cada peticion.
            new("plan", user.MembershipPlan.ToString()),
        ];

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
