using Application.DTOs;

namespace Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, Guid ownerId);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId);
    Task UpdateProjectAsync(Guid id, UpdateProjectRequest request, Guid userId);
    Task DeleteProjectAsync(Guid id, Guid userId);
    Task AddMemberToProjectAsync(Guid projectId, Guid userId, string role, Guid currentUserId);
    Task RemoveMemberFromProjectAsync(Guid projectId, Guid userId, Guid currentUserId);
    Task<IEnumerable<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId);
    Task<IEnumerable<ProjectMemberWithUserDto>> GetProjectMembersWithUsersAsync(Guid projectId);
}