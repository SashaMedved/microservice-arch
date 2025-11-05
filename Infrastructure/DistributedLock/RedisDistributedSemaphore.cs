using StackExchange.Redis;
using Domain.Interfaces;

namespace Infrastructure.DistributedLock;

public class RedisDistributedSemaphore : IDistributedSemaphore
{
    private readonly IDatabase _database;
    private readonly string _semaphoreKey;
    private readonly string _ownerId;
    private bool _isAcquired = false;
    private bool _disposed = false;

    public string Name { get; }
    public int MaxCount { get; }

    public RedisDistributedSemaphore(
        IDatabase database, 
        string name, 
        int maxCount, 
        string? ownerId = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MaxCount = maxCount > 0 ? maxCount : throw new ArgumentException("MaxCount must be greater than 0");
        
        _semaphoreKey = $"semaphore:{name}";
        _ownerId = ownerId ?? Guid.NewGuid().ToString();
    }

    public async Task<bool> WaitAsync(
        TimeSpan timeout = default, 
        CancellationToken cancellationToken = default)
    {
        if (_isAcquired)
            throw new InvalidOperationException("Semaphore already acquired");
            
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout && !cancellationToken.IsCancellationRequested)
        {
            var acquired = await TryAcquireSlotAsync();
            if (acquired)
            {
                _isAcquired = true;
                return true;
            }
            
            await Task.Delay(100, cancellationToken);
        }

        return false;
    }

    private async Task<bool> TryAcquireSlotAsync()
    {
        var transaction = _database.CreateTransaction();
        
        transaction.AddCondition(Condition.HashLengthLessThan(_semaphoreKey, MaxCount));
        
        _ = transaction.HashSetAsync(_semaphoreKey, _ownerId, DateTime.UtcNow.Ticks.ToString());
        
        _ = transaction.KeyExpireAsync(_semaphoreKey, TimeSpan.FromMinutes(10));
        
        try
        {
            var committed = await transaction.ExecuteAsync();
            return committed;
        }
        catch (RedisServerException ex) when (ex.Message.Contains("ERR condition not met"))
        {
            return false;
        }
    }

    public async Task ReleaseAsync()
    {
        if (!_isAcquired)
            throw new InvalidOperationException("Semaphore not acquired");

        await _database.HashDeleteAsync(_semaphoreKey, _ownerId);
        _isAcquired = false;
    }

    public async Task<int> GetCurrentCountAsync()
    {
        var length = await _database.HashLengthAsync(_semaphoreKey);
        return (int)length;
    }

    public async Task<bool> IsAvailableAsync()
    {
        var currentCount = await GetCurrentCountAsync();
        return currentCount < MaxCount;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed && _isAcquired)
        {
            await ReleaseAsync();
            _disposed = true;
        }
    }

    // Создаем семафор с автоматическим освобождением
    public static async Task<RedisDistributedSemaphore> CreateAndWaitAsync(
        IDatabase database,
        string name,
        int maxCount,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        var semaphore = new RedisDistributedSemaphore(database, name, maxCount);
        var acquired = await semaphore.WaitAsync(timeout, cancellationToken);
        
        if (!acquired)
            throw new TimeoutException($"Could not acquire semaphore '{name}' within timeout");
            
        return semaphore;
    }
}