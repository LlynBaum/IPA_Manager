using Ipa.Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<CriteriaProgress> CriteriaProgress { get; set; }

    /// <summary>
    /// ONLY use for Mocking!!!
    /// </summary>
    public ApplicationDbContext() : this(new DbContextOptions<ApplicationDbContext>()) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.CreatedAt)
                  .HasColumnType("TIMESTAMP")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Project Configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.UserId);
            entity.Property(p => p.CreatedAt)
                  .HasColumnType("TIMESTAMP")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(p => p.User)
                  .WithMany(u => u.Projects)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CriteriaProgress Configuration
        modelBuilder.Entity<CriteriaProgress>(entity =>
        {
            entity.HasIndex(c => c.ProjectId);
            entity.HasIndex(c => c.CriteriaId);
            entity.HasIndex(c => new { c.ProjectId, c.CriteriaId }).IsUnique();
            
            entity.Property(c => c.LastUpdated)
                  .HasColumnType("TIMESTAMP")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            entity.Property(c => c.FulfilledRequirementIds)
                  .HasColumnType("json");

            entity.HasOne(c => c.Project)
                  .WithMany(p => p.CriteriaProgress)
                  .HasForeignKey(c => c.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
