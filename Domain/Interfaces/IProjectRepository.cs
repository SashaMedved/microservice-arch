using Domain.Entities;

namespace Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId);
    Task<Project?> GetByIdWithMembersAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}