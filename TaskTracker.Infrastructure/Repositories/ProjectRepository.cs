using Microsoft.EntityFrameworkCore;
using TaskTracker.Core.Entities;
using TaskTracker.Core.Interfaces;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;

    public ProjectRepository(ProjectDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects.ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Project> AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await GetByIdAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id);
    }
    
    public async Task<Project?> GetByIdWithMembersAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}