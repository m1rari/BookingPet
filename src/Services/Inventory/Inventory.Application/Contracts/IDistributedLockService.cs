namespace Inventory.Application.Contracts;

/// <summary>
/// Service for distributed locking across multiple instances.
/// </summary>
public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan expiry);
}

