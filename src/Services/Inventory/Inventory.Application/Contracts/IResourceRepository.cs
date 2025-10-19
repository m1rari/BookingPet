using BuildingBlocks.Common.Persistence;
using Inventory.Domain.Aggregates;

namespace Inventory.Application.Contracts;

/// <summary>
/// Repository interface for Resource aggregate.
/// </summary>
public interface IResourceRepository : IRepository<Resource>
{
    Task<Resource?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Resource>> GetAvailableResourcesAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);
}

