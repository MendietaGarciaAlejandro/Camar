using Camar.Domain.Resources;

namespace Camar.Application.Abstractions;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken ct = default);
}