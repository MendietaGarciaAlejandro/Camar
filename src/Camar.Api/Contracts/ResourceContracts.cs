using Camar.Domain.Resources;

namespace Camar.Api.Contracts;

public sealed record ResourceResponse(
    Guid Id,
    string Name,
    string Type,
    int Capacity)
{
    public static ResourceResponse From(Resource resource) => new(
        resource.Id,
        resource.Name,
        resource.Type.ToString(),
        resource.Capacity);
}
