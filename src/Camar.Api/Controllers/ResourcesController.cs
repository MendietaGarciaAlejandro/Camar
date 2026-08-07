using Camar.Api.Contracts;
using Camar.Application.Abstractions;
using Camar.Application.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.Controllers;

[ApiController]
[Route("api/resources")]
public sealed class ResourcesController(
    IResourceRepository resources,
    AvailabilityService availability) : ControllerBase
{
    /// <summary>Recursos disponibles del coworking.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ResourceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceResponse>>> GetAll(CancellationToken ct)
    {
        var found = await resources.GetActiveAsync(ct);

        return Ok(found.Select(ResourceResponse.From).ToList());
    }

    /// <summary>Huecos de media hora libres de un recurso en una fecha.</summary>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType<AvailabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailability(
        Guid id,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var libres = await availability.GetFreeBlocksAsync(id, date, ct);

        return Ok(new AvailabilityResponse(
            id,
            date,
            libres.Select(b => new TimeSlot(b.Start, b.End)).ToList()));
    }
}
