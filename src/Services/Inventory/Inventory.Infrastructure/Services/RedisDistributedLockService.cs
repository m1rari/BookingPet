using Inventory.Application.Contracts;
using StackExchange.Redis;

namespace Inventory.Infrastructure.Services;

/// <summary>
/// Redis-based distributed lock service implementation.
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan expiry)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();

        // Try to acquire lock using SET NX EX (set if not exists with expiry)
        var acquired = await db.StringSetAsync(
            lockKey,
            lockValue,
            expiry,
            When.NotExists);

        if (!acquired)
        {
            return null; // Lock acquisition failed
        }

        return new RedisLock(db, lockKey, lockValue);
    }

    private class RedisLock : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;

        public RedisLock(IDatabase db, string key, string value)
        {
            _db = db;
            _key = key;
            _value = value;
        }

        public async ValueTask DisposeAsync()
        {
            // Release lock only if we still own it (compare value)
            const string releaseLockScript = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await _db.ScriptEvaluateAsync(
                releaseLockScript,
                new RedisKey[] { _key },
                new RedisValue[] { _value });
        }
    }
}

