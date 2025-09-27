using TaskTracker.Core.Entities;

namespace TaskTracker.Core.Interfaces;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(string name, string description, Guid ownerId);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId);
    Task UpdateProjectAsync(Guid id, string name, string description, Guid userId);
    Task DeleteProjectAsync(Guid id, Guid userId);
}