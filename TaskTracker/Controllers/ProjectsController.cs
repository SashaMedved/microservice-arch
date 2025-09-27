using Microsoft.AspNetCore.Mvc;
using TaskTracker.Core.Interfaces;

namespace TaskTracker.Controllers;

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
    public async Task<IActionResult> GetUserProjects()
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456"); 
        var projects = await _projectService.GetUserProjectsAsync(userId);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            var project = await _projectService.CreateProjectAsync(request.Name, request.Description, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            await _projectService.UpdateProjectAsync(id, request.Name, request.Description, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");
        
        try
        {
            await _projectService.DeleteProjectAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}