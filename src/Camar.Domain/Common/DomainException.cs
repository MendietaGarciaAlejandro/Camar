namespace Camar.Domain.Common;

/// <summary>
/// Base de los fallos de negocio. La capa Api las traduce a respuestas HTTP.
/// </summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>No existe la entidad pedida.</summary>
public sealed class NotFoundException(string message) : DomainException(message);

/// <summary>Se incumple una regla de negocio (horario, duracion, antelacion...).</summary>
public sealed class BusinessRuleException(string message) : DomainException(message);

/// <summary>La operacion choca con el estado actual de los datos.</summary>
public sealed class ConflictException(string message) : DomainException(message);

/// <summary>Credenciales invalidas.</summary>
public sealed class UnauthorizedException(string message) : DomainException(message);