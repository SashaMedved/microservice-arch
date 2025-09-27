using TaskTracker.Core.Entities;

namespace TaskTracker.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId);
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
    public interface IProjectRepository
    {
        Task<Project?> GetByIdWithMembersAsync(Guid id);
    }
}