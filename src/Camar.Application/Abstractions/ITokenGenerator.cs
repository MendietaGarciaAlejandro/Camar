using Camar.Domain.Members;

namespace Camar.Application.Abstractions;

public interface ITokenGenerator
{
    /// <summary>Emite un access token para el usuario y devuelve cuando caduca.</summary>
    (string Token, DateTimeOffset ExpiresAt) Generate(User user);
}
