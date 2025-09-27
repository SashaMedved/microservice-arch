using Microsoft.EntityFrameworkCore;
using TaskTracker.Core.Entities;

namespace TaskTracker.Infrastructure.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(p => p.Description)
                .HasColumnType("text");
                
            entity.Property(p => p.OwnerId)
                .IsRequired();
                
            entity.Property(p => p.CreatedAt)
                .IsRequired();
                
            entity.Property(p => p.UpdatedAt)
                .IsRequired();
        });
    }
}