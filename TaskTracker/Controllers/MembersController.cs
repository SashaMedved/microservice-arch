using Microsoft.AspNetCore.Mvc;
using TaskTracker.Core.Interfaces;

namespace TaskTracker.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
public class MembersController : ControllerBase
{
       private readonly IProjectMemberService _memberService;

    public MembersController(IProjectMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectMembers(Guid projectId)
    {
        var members = await _memberService.GetProjectMembersAsync(projectId);
        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
    {
        var currentUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            var member = await _memberService.AddMemberToProjectAsync(
                projectId, request.UserId, request.Role, currentUserId);
                
            return CreatedAtAction(nameof(GetProjectMembers), new { projectId }, member);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid userId, [FromBody] UpdateRoleRequest request)
    {
        var currentUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            await _memberService.UpdateMemberRoleAsync(projectId, userId, request.Role, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
    {
        var currentUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            await _memberService.RemoveMemberFromProjectAsync(projectId, userId, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class AddMemberRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}

public class UpdateRoleRequest
{
    public string Role { get; set; } = string.Empty;
}