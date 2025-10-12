using Application.Http;

namespace Application.DTOs;

public class ProjectWithUsersDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public UserDto? Owner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProjectMemberWithUserDto> Members { get; set; } = new();
}

public class ProjectMemberWithUserDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserDto? User { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}