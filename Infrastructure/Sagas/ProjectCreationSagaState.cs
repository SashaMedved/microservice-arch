using MassTransit;

namespace Infrastructure.Sagas;

public class ProjectCreationSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    
    public bool TasksCreated { get; set; }
    public bool NotificationsSetup { get; set; }
    public string? FailureReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public bool CompensationTriggered { get; set; }
}