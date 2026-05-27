namespace CanteenSystem.Application.DTOs.Menu;

public class MenuItemResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVeg { get; set; }
    public bool IsAvailable { get; set; }
    public string? Calories { get; set; }
    public int? PrepTimeMins { get; set; }
    
    // The joined property from the Category table
    public string? CategoryName { get; set; } 
}