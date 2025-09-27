using TaskTracker.Core.Entities;

namespace TaskTracker.Core.Interfaces;

public interface IProjectMemberService
{
    Task<ProjectMember> AddMemberToProjectAsync(Guid projectId, Guid userId, string role, Guid currentUserId);
    Task RemoveMemberFromProjectAsync(Guid projectId, Guid userId, Guid currentUserId);
    Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId);
    Task UpdateMemberRoleAsync(Guid projectId, Guid userId, string newRole, Guid currentUserId);
}