using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectActivity> ProjectActivities => Set<ProjectActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Description).HasField("text");
            entity.Property(p => p.OwnerId).IsRequired();
            
            entity.HasMany(p => p.Members)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId);
                  
            entity.HasMany(p => p.Activities)
                .WithOne(a => a.Project)
                .HasForeignKey(a => a.ProjectId);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => new { m.ProjectId, m.UserId }).IsUnique();
        });

        modelBuilder.Entity<ProjectActivity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Description).IsRequired();
        });
    }
}