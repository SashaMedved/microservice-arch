using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectDetailsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectDetailsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetProjectWithUsers(Guid id)
    {
        var project = await _projectService.GetProjectWithUsersAsync(id);
        return project != null ? Ok(project) : NotFound();
    }

    [HttpGet("{projectId}/members-with-users")]
    public async Task<ActionResult> GetProjectMembersWithUsers(Guid projectId)
    {
        try
        {
            var members = await _projectService.GetProjectMembersWithUsersAsync(projectId);
            return Ok(members);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}