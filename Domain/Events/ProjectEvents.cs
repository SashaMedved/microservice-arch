namespace Domain.Events;

public record ProjectCreationStarted
{
    public Guid ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public DateTime Timestamp { get; init; }
}

public record CreateProjectTasksRequested
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public DateTime Timestamp { get; init; }
}

public record SetupProjectNotificationsRequested
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProjectTasksCreated
{
    public Guid ProjectId { get; init; }
    public bool Success { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProjectNotificationsSetup
{
    public Guid ProjectId { get; init; }
    public bool Success { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProjectCreationCompleted
{
    public Guid ProjectId { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProjectCreationFailed
{
    public Guid ProjectId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}