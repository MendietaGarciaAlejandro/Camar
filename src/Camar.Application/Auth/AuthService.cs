using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Members;

namespace Camar.Application.Auth;

public sealed record AuthResult(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Role);

public class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokens,
    TimeProvider clock)
{
    public async Task<AuthResult> RegisterAsync(
        string email,
        string fullName,
        string password,
        MembershipPlan plan,
        CancellationToken ct = default)
    {
        if (await users.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Ese email ya esta registrado.");

        var user = new User(email, fullName, passwordHasher.Hash(password), plan, clock.GetUtcNow());
        await users.AddAsync(user, ct);

        return Issue(user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(email, ct);

        // Mismo error si el usuario no existe o si la contrasena falla:
        // distinguirlos permitiria averiguar que emails estan dados de alta.
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Email o contrasena incorrectos.");

        return Issue(user);
    }

    private AuthResult Issue(User user)
    {
        var (token, expiresAt) = tokens.Generate(user);

        return new AuthResult(token, expiresAt, user.Id, user.Role.ToString());
    }
}
