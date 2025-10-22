using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Sagas;

namespace Infrastructure.Data.SagaMaps;

public class ProjectCreationSagaStateMap : SagaClassMap<ProjectCreationSagaState>
{
    protected override void Configure(EntityTypeBuilder<ProjectCreationSagaState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.ProjectName).HasMaxLength(255);
        entity.Property(x => x.Description).HasColumnType("text");
        entity.Property(x => x.FailureReason).HasMaxLength(500);
        
        entity.HasIndex(x => x.ProjectId).IsUnique();
    }
}