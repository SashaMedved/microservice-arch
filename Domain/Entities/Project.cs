namespace Domain.Entities;

public class Project : EntityBase
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private readonly List<ProjectMember> _members = new();
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();
    
    private readonly List<ProjectActivity> _activities = new();
    public IReadOnlyCollection<ProjectActivity> Activities => _activities.AsReadOnly();
    
    private Project() { }

    public Project(string name, string description, Guid ownerId)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddMember(ownerId, "Owner");
        
        LogActivity(ownerId, "Created", $"Project '{name}' was created");
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMember(Guid userId, string role)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a project member");

        _members.Add(new ProjectMember(Guid.NewGuid(), Id, userId, role));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
            _members.Remove(member);
    }

    public void ChangeMemberRole(Guid userId, string newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
            member.ChangeRole(newRole);
    }
    
    private void LogActivity(Guid userId, string action, string description)
    {
        _activities.Add(new ProjectActivity(Id, userId, action, description));
    }
}