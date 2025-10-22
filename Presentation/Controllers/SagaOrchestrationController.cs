using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Domain.Events;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SagaOrchestrationController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<ProjectCreationStarted> _requestClient;

    public SagaOrchestrationController(IPublishEndpoint publishEndpoint, IRequestClient<ProjectCreationStarted> requestClient)
    {
        _publishEndpoint = publishEndpoint;
        _requestClient = requestClient;
    }

    [HttpPost("start-orchestration")]
    public async Task<ActionResult<Guid>> StartOrchestration([FromBody] StartOrchestrationRequest request)
    {
        var projectId = Guid.NewGuid();
        
        var startedEvent = new ProjectCreationStarted
        {
            ProjectId = projectId,
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            Timestamp = DateTime.UtcNow
        };
        
        await _publishEndpoint.Publish(startedEvent);

        return Accepted(new { ProjectId = projectId, Message = "Orchestration SAGA started" });
    }

    [HttpPost("start-orchestration-request")]
    public async Task<ActionResult<Guid>> StartOrchestrationWithRequest([FromBody] StartOrchestrationRequest request)
    {
        var projectId = Guid.NewGuid();
        
        var startedEvent = new ProjectCreationStarted
        {
            ProjectId = projectId,
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            Timestamp = DateTime.UtcNow
        };
        
        var response = await _requestClient.GetResponse<ProjectCreationStartedResponse>(startedEvent);

        return Ok(new { 
            ProjectId = projectId, 
            Message = "Orchestration SAGA started with request/response",
            Response = response.Message 
        });
    }
}

public class StartOrchestrationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}

public record ProjectCreationStartedResponse
{
    public Guid ProjectId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}