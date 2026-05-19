using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CanteenSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map Enums to strings in PostgreSQL
        modelBuilder.HasPostgresEnum<UserRole>();

        modelBuilder.Entity<User>(entity =>
        {
            // Enforce unique email
            entity.HasIndex(u => u.Email).IsUnique();
            
            // Store enum as a string column (varchar) in the DB
            entity.Property(e => e.Role).HasConversion<string>();

            // Global Query Filter: Automatically ignore soft-deleted users in queries
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasQueryFilter(r => !r.IsDeleted);
        });
    }

    // Auto-update the UpdatedAt field on every save
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}