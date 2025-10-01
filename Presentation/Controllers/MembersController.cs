using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IProjectService _projectService;

    public MembersController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(Guid projectId)
    {
        var members = await _projectService.GetProjectMembersAsync(projectId);
        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
    {
        var currentUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        await _projectService.AddMemberToProjectAsync(projectId, request.UserId, request.Role, currentUserId);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
    {
        var currentUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        await _projectService.RemoveMemberFromProjectAsync(projectId, userId, currentUserId);
        return NoContent();
    }
}

public class AddMemberRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}