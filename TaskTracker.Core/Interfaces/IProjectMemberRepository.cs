using TaskTracker.Core.Entities;

namespace TaskTracker.Core.Interfaces;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId);
    Task<ProjectMember?> GetByProjectAndUserAsync(Guid projectId, Guid userId);
    Task<ProjectMember> AddAsync(ProjectMember member);
    Task UpdateAsync(ProjectMember member);
    Task DeleteAsync(Guid id);
    Task<bool> IsUserMemberAsync(Guid projectId, Guid userId);
}