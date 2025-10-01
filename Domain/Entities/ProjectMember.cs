namespace Domain.Entities;

public class ProjectMember : EntityBase
{
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    
    public Project? Project { get; private set; }

    private ProjectMember() { }

    public ProjectMember(Guid id, Guid projectId, Guid userId, string role)
    {
        Id = id;
        ProjectId = projectId;
        UserId = userId;
        Role = role ?? throw new ArgumentNullException(nameof(role));
        JoinedAt = DateTime.UtcNow;
    }

    public void ChangeRole(string newRole)
    {
        Role = newRole ?? throw new ArgumentNullException(nameof(newRole));
    }
}