using Application.Http;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IHttpService _httpService;
    private readonly string _userServiceBaseUrl;

    public UserService(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _userServiceBaseUrl = configuration["Services:UserService"] ?? "http://localhost:5001";
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var url = $"{_userServiceBaseUrl}/api/users/{userId}";
            return await _httpService.GetAsync<UserDto>(url);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<UserDto>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return new List<UserDto>();

        try
        {
            var url = $"{_userServiceBaseUrl}/api/users/batch";
            var request = new { UserIds = userIds };
            return await _httpService.PostAsync<List<UserDto>>(url, request);
        }
        catch (HttpRequestException)
        {
            return new List<UserDto>();
        }
    }

    public async Task<bool> ValidateUserExistsAsync(Guid userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            return user != null;
        }
        catch
        {
            return false;
        }
    }
}