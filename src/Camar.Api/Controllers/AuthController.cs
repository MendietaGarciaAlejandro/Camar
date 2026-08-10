using System.ComponentModel.DataAnnotations;
using Camar.Application.Auth;
using Camar.Domain.Members;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.Controllers;

/// <summary>
/// Alta de socio. Ademas de las credenciales se piden los datos de facturacion: sin
/// documento fiscal no se le puede emitir una factura, y el coworking factura cada reserva.
/// La cuenta bancaria es opcional porque solo hace falta si domicilia los pagos.
/// </summary>
public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(200)] string FullName,
    [Required, MinLength(8), MaxLength(128)] string Password,
    MembershipPlan Plan,
    [Required, MaxLength(20)] string TaxId,
    [Required, MaxLength(20)] string Phone,
    [Required, MaxLength(10)] string PostalCode,
    [MaxLength(40)] string? BankAccount = null);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Role);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Alta de un socio. Siempre se crea con rol Member.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(
            request.Email,
            request.FullName,
            request.Password,
            request.Plan,
            request.TaxId,
            request.Phone,
            request.PostalCode,
            request.BankAccount,
            ct);

        return Created(string.Empty, ToResponse(result));
    }

    /// <summary>Devuelve un access token si las credenciales son correctas.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request.Email, request.Password, ct);

        return Ok(ToResponse(result));
    }

    private static AuthResponse ToResponse(AuthResult result) =>
        new(result.Token, result.ExpiresAt, result.UserId, result.Role);
}
