namespace Domain.Interfaces;

public interface IDistributedSemaphore
{
    string Name { get; }
    int MaxCount { get; }
    Task<bool> WaitAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task ReleaseAsync();
    Task<int> GetCurrentCountAsync();
    Task<bool> IsAvailableAsync();
}