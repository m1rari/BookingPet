using BuildingBlocks.Common.Persistence;
using Inventory.Application.Contracts;
using Inventory.Domain.Aggregates;
using Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Resource aggregate.
/// </summary>
public class ResourceRepository : IResourceRepository
{
    private readonly InventoryDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public ResourceRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Resource?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Use pessimistic locking with FOR UPDATE (PostgreSQL)
        return await _context.Resources
            .FromSqlRaw("SELECT * FROM \"Resources\" WHERE \"Id\" = {0} FOR UPDATE", id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .Where(r => r.Status == ResourceStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetAvailableResourcesAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        var resources = await _context.Resources
            .Where(r => r.Status == ResourceStatus.Active)
            .ToListAsync(cancellationToken);

        // Filter available resources in memory (complex query)
        return resources.Where(r => r.IsAvailableAt(startTime, endTime));
    }

    public async Task<Resource> AddAsync(Resource entity, CancellationToken cancellationToken = default)
    {
        await _context.Resources.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Resource entity)
    {
        _context.Resources.Update(entity);
    }

    public void Remove(Resource entity)
    {
        _context.Resources.Remove(entity);
    }
}

