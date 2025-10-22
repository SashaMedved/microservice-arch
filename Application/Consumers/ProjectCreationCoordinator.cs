using MassTransit;
using Domain.Events;

namespace Application.Consumers;

public class ProjectCreationCoordinator : IConsumer<ProjectCreationStarted>
{
    private readonly IProjectRepository _projectRepository;

    public ProjectCreationCoordinator(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task Consume(ConsumeContext<ProjectCreationStarted> context)
    {
        var project = await _projectRepository.GetByIdAsync(context.Message.ProjectId);
        if (project == null)
        {
            await context.Publish(new ProjectCreationFailed
            {
                ProjectId = context.Message.ProjectId,
                Reason = "Project not found",
                Timestamp = DateTime.UtcNow
            });
            return;
        }
        
        await context.Publish(new CreateProjectTasksRequested
        {
            ProjectId = context.Message.ProjectId,
            ProjectName = context.Message.Name,
            OwnerId = context.Message.OwnerId,
            Timestamp = DateTime.UtcNow
        });
        
        await context.Publish(new SetupProjectNotificationsRequested
        {
            ProjectId = context.Message.ProjectId,
            ProjectName = context.Message.Name,
            OwnerId = context.Message.OwnerId,
            Timestamp = DateTime.UtcNow
        });
    }
}

public class ProjectTasksCreator : IConsumer<CreateProjectTasksRequested>
{
    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;

    public ProjectTasksCreator(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<CreateProjectTasksRequested> context)
    {
        try
        {
            var taskServiceUrl = _configuration["Services:TaskService"] + "/api/tasks/project-setup";
            
            var result = await _httpService.PostAsync<object>(taskServiceUrl, new
            {
                context.Message.ProjectId,
                context.Message.ProjectName,
                context.Message.OwnerId
            });

            await context.Publish(new ProjectTasksCreated
            {
                ProjectId = context.Message.ProjectId,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await context.Publish(new ProjectTasksCreated
            {
                ProjectId = context.Message.ProjectId,
                Success = false,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

public class ProjectNotificationsSetup : IConsumer<SetupProjectNotificationsRequested>
{
    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;

    public ProjectNotificationsSetup(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<SetupProjectNotificationsRequested> context)
    {
        try
        {
            var notificationServiceUrl = _configuration["Services:NotificationService"] + "/api/notifications/project-setup";
            
            var result = await _httpService.PostAsync<object>(notificationServiceUrl, new
            {
                context.Message.ProjectId,
                context.Message.ProjectName,
                context.Message.OwnerId
            });

            await context.Publish(new ProjectNotificationsSetup
            {
                ProjectId = context.Message.ProjectId,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            await context.Publish(new ProjectNotificationsSetup
            {
                ProjectId = context.Message.ProjectId,
                Success = false,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

public class ProjectCreationFinalizer : IConsumer<ProjectTasksCreated>, IConsumer<ProjectNotificationsSetup>
{
    private readonly Dictionary<Guid, bool> _tasksCompleted = new();
    private readonly Dictionary<Guid, bool> _notificationsCompleted = new();

    public async Task Consume(ConsumeContext<ProjectTasksCreated> context)
    {
        _tasksCompleted[context.Message.ProjectId] = context.Message.Success;
        await CheckCompletion(context.Message.ProjectId);
    }

    public async Task Consume(ConsumeContext<ProjectNotificationsSetup> context)
    {
        _notificationsCompleted[context.Message.ProjectId] = context.Message.Success;
        await CheckCompletion(context.Message.ProjectId);
    }

    private async Task CheckCompletion(Guid projectId)
    {
        if (_tasksCompleted.TryGetValue(projectId, out var tasksSuccess) &&
            _notificationsCompleted.TryGetValue(projectId, out var notificationsSuccess))
        {
            if (tasksSuccess && notificationsSuccess)
            {
                await Bus.Publish(new ProjectCreationCompleted
                {
                    ProjectId = projectId,
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                var reason = $"Tasks: {tasksSuccess}, Notifications: {notificationsSuccess}";
                await Bus.Publish(new ProjectCreationFailed
                {
                    ProjectId = projectId,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            _tasksCompleted.Remove(projectId);
            _notificationsCompleted.Remove(projectId);
        }
    }
}