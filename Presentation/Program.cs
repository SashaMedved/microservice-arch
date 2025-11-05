using Microsoft.EntityFrameworkCore;
using Presentation.Middleware;
using MassTransit;
using Application.Http;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Application.Consumers;
using Infrastructure.Sagas;
using StackExchange.Redis;
using Domain.Interfaces;
using Infrastructure.DistributedLock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "ProjectService");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<ProjectCreationSaga, ProjectCreationSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, ProjectDbContext>((provider, builder) =>
            {
                builder.UseNpgsql(connectionString);
            });
        });
    
    x.AddConsumer<ProjectCreationCoordinator>();
    x.AddConsumer<ProjectTasksCreator>();
    x.AddConsumer<ProjectNotificationsSetup>();
    x.AddConsumer<ProjectCreationFinalizer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("project-creation-saga", e =>
        {
            e.ConfigureSaga<ProjectCreationSagaState>(context);
        });
        
        cfg.ReceiveEndpoint("project-creation-coordinator", e =>
        {
            e.ConfigureConsumer<ProjectCreationCoordinator>(context);
        });

        cfg.ReceiveEndpoint("project-tasks-creator", e =>
        {
            e.ConfigureConsumer<ProjectTasksCreator>(context);
        });

        cfg.ReceiveEndpoint("project-notifications-setup", e =>
        {
            e.ConfigureConsumer<ProjectNotificationsSetup>(context);
        });

        cfg.ReceiveEndpoint("project-creation-finalizer", e =>
        {
            e.ConfigureConsumer<ProjectCreationFinalizer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddSingleton<IDistributedSemaphoreFactory, DistributedSemaphoreFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();