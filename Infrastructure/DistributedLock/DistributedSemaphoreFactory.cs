using StackExchange.Redis;
using Domain.Interfaces;

namespace Infrastructure.DistributedLock;

public interface IDistributedSemaphoreFactory
{
    IDistributedSemaphore CreateSemaphore(string name, int maxCount);
    Task<IDistributedSemaphore> CreateAndWaitAsync(string name, int maxCount, TimeSpan timeout = default);
}

public class DistributedSemaphoreFactory : IDistributedSemaphoreFactory
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public DistributedSemaphoreFactory(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public IDistributedSemaphore CreateSemaphore(string name, int maxCount)
    {
        return new AdvancedRedisDistributedSemaphore(_database, name, maxCount);
    }

    public async Task<IDistributedSemaphore> CreateAndWaitAsync(string name, int maxCount, TimeSpan timeout = default)
    {
        var semaphore = new AdvancedRedisDistributedSemaphore(_database, name, maxCount);
        var acquired = await semaphore.WaitAsync(timeout);
        
        if (!acquired)
            throw new TimeoutException($"Could not acquire semaphore '{name}' within timeout");
            
        return semaphore;
    }
}