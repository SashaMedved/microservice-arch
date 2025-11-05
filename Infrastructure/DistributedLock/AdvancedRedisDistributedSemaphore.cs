using StackExchange.Redis;
using Domain.Interfaces;

namespace Infrastructure.DistributedLock;

public class AdvancedRedisDistributedSemaphore : IDistributedSemaphore
{
    private readonly IDatabase _database;
    private readonly string _semaphoreKey;
    private readonly string _ownersKey;
    private readonly string _ownerId;
    private readonly TimeSpan _slotTimeout;
    private Timer? _refreshTimer;
    private bool _isAcquired = false;
    private bool _disposed = false;

    public string Name { get; }
    public int MaxCount { get; }

    public AdvancedRedisDistributedSemaphore(
        IDatabase database,
        string name,
        int maxCount,
        TimeSpan slotTimeout = default,
        string? ownerId = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MaxCount = maxCount > 0 ? maxCount : throw new ArgumentException("MaxCount must be greater than 0");
        _slotTimeout = slotTimeout == default ? TimeSpan.FromMinutes(5) : slotTimeout;
        
        _semaphoreKey = $"semaphore:{name}";
        _ownersKey = $"semaphore:{name}:owners";
        _ownerId = ownerId ?? $"{Environment.MachineName}:{Guid.NewGuid()}";
    }

    public async Task<bool> WaitAsync(
        TimeSpan timeout = default, 
        CancellationToken cancellationToken = default)
    {
        if (_isAcquired)
            throw new InvalidOperationException("Semaphore already acquired");
            
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        await CleanupExpiredSlotsAsync();

        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout && !cancellationToken.IsCancellationRequested)
        {
            if (await TryAcquireSlotWithQueueAsync())
            {
                _isAcquired = true;
                StartRefreshTimer();
                return true;
            }
            
            var released = await WaitForReleaseAsync(timeout - (DateTime.UtcNow - startTime));
            if (!released)
                break;
        }

        return false;
    }

    private async Task<bool> TryAcquireSlotWithQueueAsync()
    {
        var transaction = _database.CreateTransaction();
        
        _ = transaction.SortedSetRemoveRangeByScoreAsync(_ownersKey, 0, DateTime.UtcNow.Ticks - _slotTimeout.Ticks);
        
        transaction.AddCondition(Condition.SortedSetLengthLessThan(_ownersKey, MaxCount));
        
        var score = DateTime.UtcNow.Ticks;
        _ = transaction.SortedSetAddAsync(_ownersKey, _ownerId, score);
        
        _ = transaction.HashSetAsync(_semaphoreKey, _ownerId, score.ToString());
        
        _ = transaction.KeyExpireAsync(_semaphoreKey, _slotTimeout.Add(TimeSpan.FromMinutes(1)));
        _ = transaction.KeyExpireAsync(_ownersKey, _slotTimeout.Add(TimeSpan.FromMinutes(1)));
        
        try
        {
            return await transaction.ExecuteAsync();
        }
        catch (RedisServerException ex) when (ex.Message.Contains("ERR condition not met"))
        {
            return false;
        }
    }

    private async Task<bool> WaitForReleaseAsync(TimeSpan remainingTimeout)
    {
        if (remainingTimeout <= TimeSpan.Zero)
            return false;

        try
        {
            var channel = new RedisChannel($"semaphore:{Name}:released", RedisChannel.PatternMode.Literal);
            var message = await _database.Multiplexer.GetSubscriber().SubscribeAsync(channel);
            
            using var timeoutCts = new CancellationTokenSource(remainingTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);
            
            try
            {
                await Task.Delay(Timeout.Infinite, linkedCts.Token);
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                await _database.Multiplexer.GetSubscriber().UnsubscribeAsync(channel);
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task CleanupExpiredSlotsAsync()
    {
        var expiredThreshold = DateTime.UtcNow.Ticks - _slotTimeout.Ticks;
        
        var expiredOwners = await _database.SortedSetRangeByScoreAsync(_ownersKey, 0, expiredThreshold);
        if (expiredOwners.Length > 0)
        {
            var transaction = _database.CreateTransaction();
            _ = transaction.SortedSetRemoveAsync(_ownersKey, expiredOwners);
            _ = transaction.HashDeleteAsync(_semaphoreKey, expiredOwners.Select(o => (RedisValue)o.ToString()).ToArray());
            await transaction.ExecuteAsync();
            
            if (expiredOwners.Length > 0)
            {
                var channel = new RedisChannel($"semaphore:{Name}:released", RedisChannel.PatternMode.Literal);
                await _database.Multiplexer.GetSubscriber().PublishAsync(channel, $"slots_released:{expiredOwners.Length}");
            }
        }
    }

    private void StartRefreshTimer()
    {
        _refreshTimer = new Timer(async _ => await RefreshSlotAsync(), null, 
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task RefreshSlotAsync()
    {
        if (!_isAcquired || _disposed) return;

        try
        {
            var transaction = _database.CreateTransaction();
            var score = DateTime.UtcNow.Ticks;
            
            _ = transaction.SortedSetAddAsync(_ownersKey, _ownerId, score, When.Always);
            _ = transaction.HashSetAsync(_semaphoreKey, _ownerId, score.ToString());
            
            await transaction.ExecuteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing semaphore slot: {ex.Message}");
        }
    }

    public async Task ReleaseAsync()
    {
        if (!_isAcquired)
            throw new InvalidOperationException("Semaphore not acquired");

        var transaction = _database.CreateTransaction();
        _ = transaction.SortedSetRemoveAsync(_ownersKey, _ownerId);
        _ = transaction.HashDeleteAsync(_semaphoreKey, _ownerId);
        
        await transaction.ExecuteAsync();
        
        _refreshTimer?.Dispose();
        _isAcquired = false;
        
        var channel = new RedisChannel($"semaphore:{Name}:released", RedisChannel.PatternMode.Literal);
        await _database.Multiplexer.GetSubscriber().PublishAsync(channel, "slot_released");
    }

    public async Task<int> GetCurrentCountAsync()
    {
        await CleanupExpiredSlotsAsync();
        var length = await _database.SortedSetLengthAsync(_ownersKey);
        return (int)length;
    }

    public async Task<bool> IsAvailableAsync()
    {
        await CleanupExpiredSlotsAsync();
        var currentCount = await GetCurrentCountAsync();
        return currentCount < MaxCount;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_isAcquired)
            {
                await ReleaseAsync();
            }
            
            _refreshTimer?.Dispose();
            _disposed = true;
        }
    }
}