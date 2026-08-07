using System.ComponentModel.DataAnnotations;
using Camar.Api.Contracts;
using Camar.Application.Abstractions;
using Camar.Domain.Common;
using Camar.Domain.Members;
using Camar.Domain.Resources;
using Camar.Domain.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.Controllers;

public sealed record CreateResourceRequest(
    [Required, MaxLength(100)] string Name,
    ResourceType Type,
    [Range(1, 500)] int Capacity);

public sealed record BlockDayRequest(
    DateOnly Date,
    [Required, MaxLength(200)] string Reason);

public sealed record BlockedDayResponse(Guid Id, DateOnly Date, string Reason);

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/admin")]
public sealed class AdminController(
    IResourceRepository resources,
    IReservationRepository reservations,
    IBlockedDayRepository blockedDays,
    TimeProvider clock) : ControllerBase
{
    /// <summary>Da de alta un recurso nuevo.</summary>
    [HttpPost("resources")]
    [ProducesResponseType<ResourceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ResourceResponse>> CreateResource(
        CreateResourceRequest request,
        CancellationToken ct)
    {
        var resource = new Resource(request.Name, request.Type, request.Capacity);
        await resources.AddAsync(resource, ct);

        return Created(string.Empty, ResourceResponse.From(resource));
    }

    /// <summary>Baja logica de un recurso: deja de poder reservarse pero conserva su historial.</summary>
    [HttpDelete("resources/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivateResource(Guid id, CancellationToken ct)
    {
        var resource = await resources.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe el recurso {id}.");

        try
        {
            resource.Deactivate();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await resources.UpdateAsync(resource, ct);

        return NoContent();
    }

    /// <summary>Todas las reservas del coworking, opcionalmente filtradas por recurso.</summary>
    [HttpGet("reservations")]
    [ProducesResponseType<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetAllReservations(
        [FromQuery] Guid? resourceId,
        CancellationToken ct)
    {
        var found = await reservations.GetAllAsync(resourceId, ct);

        return Ok(found.Select(ReservationResponse.From).ToList());
    }

    /// <summary>Dias en los que el coworking no abre.</summary>
    [HttpGet("blocked-days")]
    [ProducesResponseType<IReadOnlyList<BlockedDayResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BlockedDayResponse>>> GetBlockedDays(CancellationToken ct)
    {
        var found = await blockedDays.GetAllAsync(ct);

        return Ok(found.Select(b => new BlockedDayResponse(b.Id, b.Date, b.Reason)).ToList());
    }

    /// <summary>Bloquea un dia (festivo, obras, cierre).</summary>
    [HttpPost("blocked-days")]
    [ProducesResponseType<BlockedDayResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BlockedDayResponse>> BlockDay(
        BlockDayRequest request,
        CancellationToken ct)
    {
        if (await blockedDays.GetByDateAsync(request.Date, ct) is not null)
            throw new ConflictException($"El {request.Date:dd/MM/yyyy} ya estaba bloqueado.");

        var blockedDay = new BlockedDay(request.Date, request.Reason, clock.GetUtcNow());
        await blockedDays.AddAsync(blockedDay, ct);

        return Created(
            string.Empty,
            new BlockedDayResponse(blockedDay.Id, blockedDay.Date, blockedDay.Reason));
    }

    /// <summary>Vuelve a abrir un dia bloqueado.</summary>
    [HttpDelete("blocked-days/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockDay(Guid id, CancellationToken ct)
    {
        if (!await blockedDays.RemoveAsync(id, ct))
            throw new NotFoundException($"No existe el dia bloqueado {id}.");

        return NoContent();
    }
}
