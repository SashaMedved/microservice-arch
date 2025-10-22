using MassTransit;
using Domain.Events;

namespace Infrastructure.Sagas;

public class ProjectCreationSaga : MassTransitStateMachine<ProjectCreationSagaState>
{
    public State Creating { get; private set; } = null!;
    public State CreatingTasks { get; private set; } = null!;
    public State SettingUpNotifications { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;
    
    public Event<ProjectCreationStarted> CreationStarted { get; private set; } = null!;
    public Event<ProjectTasksCreated> TasksCreated { get; private set; } = null!;
    public Event<ProjectNotificationsSetup> NotificationsSetup { get; private set; } = null!;
    public Event<ProjectCreationFailed> CreationFailed { get; private set; } = null!;

    public ProjectCreationSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CreationStarted, e => e.CorrelateById(context => context.Message.ProjectId));
        Event(() => TasksCreated, e => e.CorrelateById(context => context.Message.ProjectId));
        Event(() => NotificationsSetup, e => e.CorrelateById(context => context.Message.ProjectId));
        Event(() => CreationFailed, e => e.CorrelateById(context => context.Message.ProjectId));
        
        Initially(
            When(CreationStarted)
                .Then(context =>
                {
                    context.Saga.ProjectId = context.Message.ProjectId;
                    context.Saga.ProjectName = context.Message.Name;
                    context.Saga.Description = context.Message.Description;
                    context.Saga.OwnerId = context.Message.OwnerId;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .PublishAsync(context => context.Init<CreateProjectTasksRequested>(new
                {
                    ProjectId = context.Saga.ProjectId,
                    ProjectName = context.Saga.ProjectName,
                    OwnerId = context.Saga.OwnerId,
                    Timestamp = DateTime.UtcNow
                }))
                .TransitionTo(CreatingTasks)
        );
        
        During(CreatingTasks,
            When(TasksCreated)
                .Then(context =>
                {
                    context.Saga.TasksCreated = context.Message.Success;
                })
                .If(context => context.Message.Success,
                    then => then
                        .PublishAsync(context => context.Init<SetupProjectNotificationsRequested>(new
                        {
                            ProjectId = context.Saga.ProjectId,
                            ProjectName = context.Saga.ProjectName,
                            OwnerId = context.Saga.OwnerId,
                            Timestamp = DateTime.UtcNow
                        }))
                        .TransitionTo(SettingUpNotifications),
                    else => then
                        .PublishAsync(context => context.Init<ProjectCreationFailed>(new
                        {
                            ProjectId = context.Saga.ProjectId,
                            Reason = "Failed to create project tasks",
                            Timestamp = DateTime.UtcNow
                        }))
                        .TransitionTo(Failed)
                )
        );
                    
        During(SettingUpNotifications,
            When(NotificationsSetup)
                .Then(context =>
                {
                    context.Saga.NotificationsSetup = context.Message.Success;
                })
                .If(context => context.Message.Success,
                    then => then
                        .Then(context =>
                        {
                            context.Saga.CompletedAt = DateTime.UtcNow;
                        })
                        .PublishAsync(context => context.Init<ProjectCreationCompleted>(new
                        {
                            ProjectId = context.Saga.ProjectId,
                            Timestamp = DateTime.UtcNow
                        }))
                        .TransitionTo(Completed),
                    else => then
                        .PublishAsync(context => context.Init<ProjectCreationFailed>(new
                        {
                            ProjectId = context.Saga.ProjectId,
                            Reason = "Failed to setup project notifications",
                            Timestamp = DateTime.UtcNow
                        }))
                        .TransitionTo(Failed)
                )
        );
                    
        DuringAny(
            When(CreationFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Failed)
        );
        
        SetCompletedWhenFinalized();
    }
}