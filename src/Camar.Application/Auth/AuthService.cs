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
        string taxId,
        string phone,
        string postalCode,
        string? bankAccount,
        CancellationToken ct = default)
    {
        if (await users.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Ese email ya esta registrado.");

        // Los objetos de valor validan al construirse, asi que un documento mal formado
        // se rechaza aqui y nunca llega a la base de datos.
        var documento = new TaxId(taxId);
        var telefono = new PhoneNumber(phone);
        var codigoPostal = new PostalCode(postalCode);
        var cuenta = string.IsNullOrWhiteSpace(bankAccount)
            ? (BankAccount?)null
            : new BankAccount(bankAccount);

        var user = new User(
            email,
            fullName,
            passwordHasher.Hash(password),
            plan,
            documento,
            telefono,
            codigoPostal,
            clock.GetUtcNow(),
            cuenta);
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
