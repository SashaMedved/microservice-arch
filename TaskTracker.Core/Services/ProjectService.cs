using TaskTracker.Core.Entities;
using TaskTracker.Core.Interfaces;

namespace TaskTracker.Core.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Project> CreateProjectAsync(string name, string description, Guid ownerId)
    {
        // Бизнес-логика: валидация
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _projectRepository.AddAsync(project);
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId)
    {
        return await _projectRepository.GetByOwnerIdAsync(userId);
    }

    public async Task UpdateProjectAsync(Guid id, string name, string description, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            throw new KeyNotFoundException("Project not found");
        
        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only edit");

        project.Name = name.Trim();
        project.Description = description?.Trim() ?? string.Empty;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
    }

    public async Task DeleteProjectAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only delete");

        await _projectRepository.DeleteAsync(id);
    }
}