using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects()
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        var projects = await _projectService.GetUserProjectsAsync(userId);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return project != null ? Ok(project) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest request)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        var project = await _projectService.CreateProjectAsync(request, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequest request)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        await _projectService.UpdateProjectAsync(id, request, userId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        await _projectService.DeleteProjectAsync(id, userId);
        return NoContent();
    }
}