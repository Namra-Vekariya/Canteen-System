namespace CanteenSystem.Domain.Entities;

public class MenuItem : BaseEntity
{
    public required Guid CategoryId { get; set; }
    public required string Name { get; set; }            
    public required string Slug { get; set; }            
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVeg { get; set; } = true;
    public bool IsAvailable { get; set; } = true;       
    public string? Calories { get; set; }             
    public int? PrepTimeMins { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ICollection<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}