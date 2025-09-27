using TaskTracker.Core.Entities;
using TaskTracker.Core.Interfaces;

namespace TaskTracker.Core.Services;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectMemberService(IProjectMemberRepository memberRepository, IProjectRepository projectRepository)
    {
        _memberRepository = memberRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ProjectMember> AddMemberToProjectAsync(Guid projectId, Guid userId, string role, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");
        
        if (project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only project owner can add members");
        
        var existingMember = await _memberRepository.GetByProjectAndUserAsync(projectId, userId);
        if (existingMember != null)
            throw new InvalidOperationException("User is already a project member");

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        return await _memberRepository.AddAsync(member);
    }

    public async Task RemoveMemberFromProjectAsync(Guid projectId, Guid userId, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");
        
        if (project.OwnerId != currentUserId && userId != currentUserId)
            throw new UnauthorizedAccessException("You can only remove yourself from the project");

        var member = await _memberRepository.GetByProjectAndUserAsync(projectId, userId);
        if (member == null)
            throw new KeyNotFoundException("Member not found");
        
        if (project.OwnerId == userId && userId == currentUserId)
            throw new InvalidOperationException("Project owner cannot remove themselves");

        await _memberRepository.DeleteAsync(member.Id);
    }

    public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId)
    {
        return await _memberRepository.GetByProjectIdAsync(projectId);
    }

    public async Task UpdateMemberRoleAsync(Guid projectId, Guid userId, string newRole, Guid currentUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != currentUserId)
            throw new UnauthorizedAccessException("Only project owner can change roles");

        var member = await _memberRepository.GetByProjectAndUserAsync(projectId, userId);
        if (member == null)
            throw new KeyNotFoundException("Member not found");

        member.Role = newRole;
        await _memberRepository.UpdateAsync(member);
    }
}