using BuildingBlocks.Common.Domain;

namespace BuildingBlocks.Common.Persistence;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}

