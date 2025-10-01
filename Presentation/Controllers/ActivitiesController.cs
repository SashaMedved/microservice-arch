using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace Presentation.Controllers;


[ApiController]
[Route("api/projects/{projectId}/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly ProjectDbContext _context;

    public ActivitiesController(ProjectDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivities(Guid projectId)
    {
        var activities = await _context.ProjectActivities
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(50)
            .ToListAsync();

        return Ok(activities);
    }
}