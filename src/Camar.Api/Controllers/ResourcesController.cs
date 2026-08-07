using Camar.Api.Contracts;
using Camar.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Camar.Api.Controllers;

[ApiController]
[Route("api/resources")]
public sealed class ResourcesController(IResourceRepository resources) : ControllerBase
{
    /// <summary>Recursos disponibles del coworking.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ResourceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResourceResponse>>> GetAll(CancellationToken ct)
    {
        var found = await resources.GetActiveAsync(ct);

        return Ok(found.Select(ResourceResponse.From).ToList());
    }
}
