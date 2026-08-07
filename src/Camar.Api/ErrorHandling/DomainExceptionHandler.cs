using Camar.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.ErrorHandling;

/// <summary>
/// Convierte las excepciones de dominio en respuestas ProblemDetails.
/// Lo que no sea DomainException se deja pasar: son fallos de verdad y deben dar 500.
/// </summary>
public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not DomainException domainException)
            return false;

        var (status, title) = domainException switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida"),
            _ => (StatusCodes.Status400BadRequest, "Peticion invalida"),
        };

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = domainException,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = domainException.Message,
            },
        });
    }
}
