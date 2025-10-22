using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Domain.Events;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SagaCoordinationController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SagaCoordinationController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("start-coordination")]
    public async Task<ActionResult<Guid>> StartCoordination([FromBody] StartCoordinationRequest request)
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

        return Accepted(new { ProjectId = projectId, Message = "Coordination SAGA started" });
    }
}

public class StartCoordinationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}