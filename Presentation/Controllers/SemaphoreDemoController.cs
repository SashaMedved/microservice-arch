using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemaphoreDemoController : ControllerBase
{
    private readonly IDistributedSemaphoreFactory _semaphoreFactory;
    private readonly ILogger<SemaphoreDemoController> _logger;

    public SemaphoreDemoController(
        IDistributedSemaphoreFactory semaphoreFactory,
        ILogger<SemaphoreDemoController> logger)
    {
        _semaphoreFactory = semaphoreFactory;
        _logger = logger;
    }

    [HttpPost("limited-operation")]
    public async Task<ActionResult> PerformLimitedOperation()
    {
        await using var semaphore = await _semaphoreFactory.CreateAndWaitAsync(
            "limited-operation", 
            maxCount: 3, 
            timeout: TimeSpan.FromSeconds(10));

        try
        {
            _logger.LogInformation("Acquired semaphore for limited operation. Current count: {Count}", 
                await semaphore.GetCurrentCountAsync());
            
            await Task.Delay(5000);
            
            _logger.LogInformation("Completed limited operation");
            return Ok(new { Message = "Operation completed successfully", Timestamp = DateTime.UtcNow });
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning("Could not acquire semaphore: {Message}", ex.Message);
            return StatusCode(429, new { Error = "Too many concurrent requests" });
        }
    }

    [HttpGet("semaphore-status/{name}")]
    public async Task<ActionResult> GetSemaphoreStatus(string name)
    {
        var semaphore = _semaphoreFactory.CreateSemaphore(name, 1);
        
        return Ok(new
        {
            Name = name,
            CurrentCount = await semaphore.GetCurrentCountAsync(),
            IsAvailable = await semaphore.IsAvailableAsync(),
            MaxCount = semaphore.MaxCount
        });
    }

    [HttpPost("batch-processing")]
    public async Task<ActionResult> StartBatchProcessing()
    {
        await using var semaphore = _semaphoreFactory.CreateSemaphore("batch-processing", 2);
        
        var acquired = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        if (!acquired)
        {
            return StatusCode(429, new { Error = "Batch processing slots are full" });
        }

        try
        {
            // Имитируем обработку пакета данных
            await ProcessBatchAsync();
            
            return Ok(new { Message = "Batch processing completed" });
        }
        finally
        {
            await semaphore.ReleaseAsync();
        }
    }

    private async Task ProcessBatchAsync()
    {
        _logger.LogInformation("Starting batch processing...");
        await Task.Delay(3000);
        _logger.LogInformation("Batch processing completed");
    }
}