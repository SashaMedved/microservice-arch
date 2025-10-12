using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserService _userService;

    public ProjectService(IProjectRepository projectRepository, IUserService userService)
    {
        _projectRepository = projectRepository;
        _userService = userService;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid ownerId)
    {
        var project = new Project(request.Name, request.Description, ownerId);
        var createdProject = await _projectRepository.AddAsync(project);
        return MapToDto(createdProject);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(id);
        return project != null ? MapToDto(project) : null;
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId)
    {
        var projects = await _projectRepository.GetByOwnerIdAsync(userId);
        return projects.Select(MapToDto);
    }

    public async Task UpdateProjectAsync(Guid id, UpdateProjectRequest request, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only project owner can update project");

        project.Update(request.Name, request.Description);
        await _projectRepository.UpdateAsync(project);
    }

    public async Task DeleteProjectAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only project owner can delete project");

        await _projectRepository.DeleteAsync(id);
    }

    public async Task AddMemberToProjectAsync(Guid projectId, Guid userId, string role, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only project owner can add members");

        project.AddMember(userId, role);
        await _projectRepository.UpdateAsync(project);
    }

    public async Task RemoveMemberFromProjectAsync(Guid projectId, Guid userId, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != currentUserId && userId != currentUserId)
            throw new UnauthorizedAccessException("You can only remove yourself from the project");

        project.RemoveMember(userId);
        await _projectRepository.UpdateAsync(project);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        return project.Members.Select(m => new ProjectMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    private ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = project.Members.Select(m => new ProjectMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList()
        };
    }
    
    public async Task<ProjectWithUsersDto?> GetProjectWithUsersAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(id);
        if (project == null)
            return null;

        var userIds = project.Members.Select(m => m.UserId).ToList();
        userIds.Add(project.OwnerId);
        userIds = userIds.Distinct().ToList();

        var users = await _userService.GetUsersByIdsAsync(userIds);

        return new ProjectWithUsersDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            Owner = users.FirstOrDefault(u => u.Id == project.OwnerId),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = project.Members.Select(m => new ProjectMemberWithUserDto
            {
                Id = m.Id,
                UserId = m.UserId,
                User = users.FirstOrDefault(u => u.Id == m.UserId),
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList()
        };
    }

    public async Task<IEnumerable<ProjectMemberWithUserDto>> GetProjectMembersWithUsersAsync(Guid projectId)
    {
        var project = await _projectRepository.GetByIdWithMembersAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        var userIds = project.Members.Select(m => m.UserId).Distinct().ToList();
        var users = await _userService.GetUsersByIdsAsync(userIds);

        return project.Members.Select(m => new ProjectMemberWithUserDto
        {
            Id = m.Id,
            UserId = m.UserId,
            User = users.FirstOrDefault(u => u.Id == m.UserId),
            Role = m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    public async Task AddMemberToProjectAsync(Guid projectId, Guid userId, string role, Guid currentUserId)
    {
        var userExists = await _userService.ValidateUserExistsAsync(userId);
        if (!userExists)
            throw new ArgumentException("User not found");

        var project = await _projectRepository.GetByIdWithMembersAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only project owner can add members");

        project.AddMember(userId, role);
        await _projectRepository.UpdateAsync(project);
    }
}