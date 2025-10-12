using Application.Http;

namespace Application.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<List<UserDto>> GetUsersByIdsAsync(List<Guid> userIds);
    Task<bool> ValidateUserExistsAsync(Guid userId);
}