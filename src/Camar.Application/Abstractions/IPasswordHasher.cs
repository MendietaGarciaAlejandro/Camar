namespace Camar.Application.Abstractions;

/// <summary>
/// El dominio guarda un hash opaco; que algoritmo lo produce es cosa de Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
