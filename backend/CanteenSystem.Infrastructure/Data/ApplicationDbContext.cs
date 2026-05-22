using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CanteenSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ── Auth ──────────────────────────────────────────────────────────────
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Otp> Otps { get; set; } = null!;

    // ── Menu ──────────────────────────────────────────────────────────────
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<DailyMenu> DailyMenus { get; set; } = null!;

    // ── Cart ──────────────────────────────────────────────────────────────
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;

    // ── Orders ────────────────────────────────────────────────────────────
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderStatusLog> OrderStatusLogs { get; set; } = null!;

    // ── Payments & Notifications ──────────────────────────────────────────
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
        ConfigureOtp(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureMenuItem(modelBuilder);
        ConfigureDailyMenu(modelBuilder);
        ConfigureCart(modelBuilder);
        ConfigureCartItem(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
        ConfigureOrderStatusLog(modelBuilder);
        ConfigurePayment(modelBuilder);
        ConfigureNotification(modelBuilder);
    }

    // ── Entity Configurations ─────────────────────────────────────────────

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);  // FIX: was double ;;
            entity.Property(e => e.Name).HasMaxLength(100);                          // FIX: 150 → 100
            entity.Property(e => e.Email).HasMaxLength(255);                         // RFC 5321 standard
            entity.Property(e => e.Phone).HasMaxLength(16);                          // E.164 with "+"

            entity.HasQueryFilter(u => !u.IsDeleted);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(r => r.TokenHash).IsUnique();
            entity.HasIndex(r => r.Family);

            // FIX: Added missing MaxLength constraints
            entity.Property(e => e.TokenHash).HasMaxLength(64);      // SHA-256 = exactly 64 hex chars
            entity.Property(e => e.Family).HasMaxLength(36);          // Guid string = 36 chars
            entity.Property(e => e.IpAddress).HasMaxLength(45);       // IPv6 max = 45 chars
            entity.Property(e => e.RevokedReason).HasMaxLength(255);

            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOtp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Otp>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);  // FIX: was double ;;
            entity.Property(e => e.OtpHash).HasMaxLength(64);                        // FIX: SHA-256 hash
            entity.Property(e => e.Attempts).HasDefaultValue(0);

            entity.HasIndex(o => new { o.UserId, o.Type, o.IsUsed });

            entity.HasQueryFilter(o => !o.IsDeleted);

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Otps)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);     // FIX: 150 → 100
            entity.Property(e => e.Slug).HasMaxLength(100);     // FIX: 150 → 100
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasQueryFilter(c => !c.IsDeleted);
        });
    }

    private static void ConfigureMenuItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasIndex(m => m.Slug).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(150);     // FIX: 200 → 150
            entity.Property(e => e.Slug).HasMaxLength(150);     // FIX: 200 → 150
            entity.Property(e => e.Calories).HasMaxLength(50);  // e.g., "250 kcal"
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasQueryFilter(m => !m.IsDeleted);

            entity.HasOne(m => m.Category)
                  .WithMany(c => c.MenuItems)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDailyMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyMenu>(entity =>
        {
            entity.Property(e => e.MealSlot).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);

            entity.HasIndex(d => new { d.MenuDate, d.MenuItemId, d.MealSlot }).IsUnique();

            entity.HasQueryFilter(d => !d.IsDeleted);

            entity.HasOne(d => d.MenuItem)
                  .WithMany(m => m.DailyMenus)
                  .HasForeignKey(d => d.MenuItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCart(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(c => c.UserId);

            entity.HasQueryFilter(c => !c.IsDeleted);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.Carts)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCartItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(ci => new { ci.CartId, ci.MenuItemId }).IsUnique();

            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasQueryFilter(ci => !ci.IsDeleted);

            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.CartItems)
                  .HasForeignKey(ci => ci.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.MenuItem)
                  .WithMany(m => m.CartItems)
                  .HasForeignKey(ci => ci.MenuItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.Token).IsUnique();
            entity.HasIndex(o => o.UserId);

            // FIX: Added missing HasMaxLength for enum string conversions
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(20);

            entity.Property(e => e.Token).HasMaxLength(15);                          // FIX: 20 → 15
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(500);           // FIX: was missing
            entity.Property(e => e.CancellationReason).HasMaxLength(500);            // FIX: was missing

            entity.HasQueryFilter(o => !o.IsDeleted);

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.ItemName).HasMaxLength(150);   // FIX: 200 → 150 (align with MenuItem)
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.LineTotal).HasPrecision(10, 2);

            entity.HasQueryFilter(oi => !oi.IsDeleted);

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.MenuItem)
                  .WithMany(m => m.OrderItems)
                  .HasForeignKey(oi => oi.MenuItemId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureOrderStatusLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderStatusLog>(entity =>
        {
            entity.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(osl => osl.OrderId);

            // ⚠️ NO HasQueryFilter — audit log, never hide history

            entity.HasOne(osl => osl.Order)
                  .WithMany(o => o.StatusLogs)
                  .HasForeignKey(osl => osl.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(osl => osl.ChangedByUser)
                  .WithMany()
                  .HasForeignKey(osl => osl.ChangedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.OrderId).IsUnique();
            entity.HasIndex(p => p.GatewayTxnId);

            entity.Property(e => e.Method).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(10, 2);
            entity.Property(e => e.Gateway).HasMaxLength(50);
            entity.Property(e => e.GatewayTxnId).HasMaxLength(255);
            entity.Property(e => e.GatewayOrderId).HasMaxLength(255);
            entity.Property(e => e.FailureReason).HasMaxLength(500);                 // FIX: was missing

            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.HasOne(p => p.Order)
                  .WithOne(o => o.Payment)
                  .HasForeignKey<Payment>(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.User)
                  .WithMany(u => u.Payments)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Message).HasMaxLength(1000);                      // FIX: was missing
            entity.Property(e => e.Data).HasColumnType("jsonb");

            entity.HasIndex(n => new { n.UserId, n.IsRead });

            entity.HasQueryFilter(n => !n.IsDeleted);

            entity.HasOne(n => n.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ── Auto-update UpdatedAt on every save ───────────────────────────────
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