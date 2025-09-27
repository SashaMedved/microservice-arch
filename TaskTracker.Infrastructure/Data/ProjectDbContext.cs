using Microsoft.EntityFrameworkCore;
using TaskTracker.Core.Entities;

namespace TaskTracker.Infrastructure.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Description).HasColumnType("text");
            entity.Property(p => p.OwnerId).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).IsRequired();
            
            entity.HasMany(p => p.Members)
                .WithOne(pm => pm.Project)
                .HasForeignKey(pm => pm.ProjectId);
        });
        
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => pm.Id);
            
            entity.Property(pm => pm.ProjectId).IsRequired();
            entity.Property(pm => pm.UserId).IsRequired();
            entity.Property(pm => pm.Role).IsRequired().HasMaxLength(50);
            entity.Property(pm => pm.JoinedAt).IsRequired();
            
            entity.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();
        });
    }
}