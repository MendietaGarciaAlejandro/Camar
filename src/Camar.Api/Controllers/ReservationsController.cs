using Camar.Api.Contracts;
using Camar.Application.Abstractions;
using Camar.Application.Reservations;
using Camar.Domain.Common;
using Camar.Domain.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public sealed class ReservationsController(
    ReservationService reservations,
    IReservationRepository repository) : ControllerBase
{
    /// <summary>Crea una reserva confirmada si cumple todas las reglas del coworking.</summary>
    [HttpPost]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReservationResponse>> Create(
        CreateReservationRequest request,
        CancellationToken ct)
    {
        // El constructor de Period valida que el fin sea posterior al inicio.
        // No se reusa el mensaje de la excepcion: lleva el nombre del parametro de C#.
        Period period;
        try
        {
            period = new Period(request.Start, request.End);
        }
        catch (ArgumentException)
        {
            throw new BusinessRuleException("El fin de la reserva debe ser posterior al inicio.");
        }

        var reservation = await reservations.CreateAsync(
            User.GetUserId(), request.ResourceId, period, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            ReservationResponse.From(reservation));
    }

    /// <summary>Devuelve una reserva propia.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id, CancellationToken ct)
    {
        var reservation = await repository.GetByIdAsync(id, ct);

        // Las reservas ajenas se tratan como inexistentes para no filtrar ids validos.
        if (reservation is null || reservation.UserId != User.GetUserId())
            throw new NotFoundException($"No existe la reserva {id}.");

        return Ok(ReservationResponse.From(reservation));
    }

    /// <summary>Cancela una reserva propia. Devuelve el importe reembolsado.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ReservationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Cancel(Guid id, CancellationToken ct)
    {
        var reservation = await reservations.CancelAsync(id, User.GetUserId(), ct);

        return Ok(ReservationResponse.From(reservation));
    }

    /// <summary>Reservas del usuario autenticado, de la mas reciente a la mas antigua.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetMine(CancellationToken ct)
    {
        var found = await repository.GetByUserAsync(User.GetUserId(), ct);

        return Ok(found.Select(ReservationResponse.From).ToList());
    }
}
