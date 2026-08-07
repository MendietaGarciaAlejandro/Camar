using Camar.Domain.Resources;

namespace Camar.Application.Abstractions;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Resource>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(Resource resource, CancellationToken ct = default);
    Task UpdateAsync(Resource resource, CancellationToken ct = default);
}
