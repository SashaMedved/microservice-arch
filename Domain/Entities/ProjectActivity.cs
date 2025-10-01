namespace Domain.Entities;

public class ProjectActivity : EntityBase
{
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; }
    public string Description { get; private set; }
    public DateTime OccurredAt { get; private set; }
    
    public Project? Project { get; private set; }

    private ProjectActivity() { }

    public ProjectActivity(Guid projectId, Guid userId, string action, string description)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        UserId = userId;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        OccurredAt = DateTime.UtcNow;
    }
}