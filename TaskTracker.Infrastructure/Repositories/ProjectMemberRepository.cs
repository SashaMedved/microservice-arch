using Microsoft.EntityFrameworkCore;
using TaskTracker.Core.Entities;
using TaskTracker.Core.Interfaces;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly ProjectDbContext _context;

    public ProjectMemberRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMember?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectMembers.FindAsync(id);
    }

    public async Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<ProjectMember?> GetByProjectAndUserAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task<ProjectMember> AddAsync(ProjectMember member)
    {
        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateAsync(ProjectMember member)
    {
        _context.ProjectMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var member = await GetByIdAsync(id);
        if (member != null)
        {
            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }
}