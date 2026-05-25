using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanteenSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Apply any pending migrations automatically (great for dev)
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding Users...");
                await SeedUsersAsync(context);
            }

            if (!await context.Categories.AnyAsync())
            {
                logger.LogInformation("Seeding Categories and Menu Items...");
                await SeedMenuAsync(context);
            }

            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        // Password for both is "Password123!"
        var defaultHash = BCrypt.Net.BCrypt.HashPassword("admin@123");

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "System Admin",
            Email = "admin@canteen.com",
            PasswordHash = defaultHash,
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerifiedAt = DateTime.UtcNow
        };

        var customer = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "user@canteen.com",
            PasswordHash = defaultHash,
            Role = UserRole.User,
            IsActive = true,
            EmailVerifiedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(admin, customer);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMenuAsync(ApplicationDbContext context)
    {
        var breakfast = new Category { Id = Guid.NewGuid(), Name = "Breakfast", Slug = "breakfast", DisplayOrder = 1 };
        var lunch = new Category { Id = Guid.NewGuid(), Name = "Lunch", Slug = "lunch", DisplayOrder = 2 };
        var snacks = new Category { Id = Guid.NewGuid(), Name = "Snacks", Slug = "snacks", DisplayOrder = 3 };

        await context.Categories.AddRangeAsync(breakfast, lunch, snacks);

        var menuItems = new List<MenuItem>
        {
            new MenuItem { CategoryId = breakfast.Id, Name = "Poha", Slug = "poha", Price = 30.00m, IsVeg = true, ImageUrl = "https://media.istockphoto.com/id/1093261264/photo/aloo-kanda-poha-or-tarri-pohe-with-spicy-chana-masala-curry-selective-focus.jpg?s=2048x2048&w=is&k=20&c=xKoCqqLZTditwRvnnFH5h3Qkc2A51CfGciSUzDOajco=" },
            new MenuItem { CategoryId = breakfast.Id, Name = "Masala Dhosa", Slug = "masala-dosa", Price = 60.00m, IsVeg = true, ImageUrl = "https://images.unsplash.com/photo-1668236543090-82eba5ee5976?w=500&q=80" },
            
            new MenuItem { CategoryId = lunch.Id, Name = "Veg Thali", Slug = "veg-thali", Price = 120.00m, IsVeg = true, ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&q=80" },
            new MenuItem { CategoryId = lunch.Id, Name = "Chicken Biryani", Slug = "chicken-biryani", Price = 150.00m, IsVeg = false, ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=500&q=80" },
            
            new MenuItem { CategoryId = snacks.Id, Name = "Samosa (2 pcs)", Slug = "samosa", Price = 20.00m, IsVeg = true, ImageUrl = "https://images.unsplash.com/photo-1601050690597-df0568a70950?w=500&q=80" },
            new MenuItem { CategoryId = snacks.Id, Name = "French Fries", Slug = "french-fries", Price = 50.00m, IsVeg = true, ImageUrl = "https://images.unsplash.com/photo-1576107232684-1279f390859f?w=500&q=80" }
        };

        await context.MenuItems.AddRangeAsync(menuItems);
        await context.SaveChangesAsync();
    }
}