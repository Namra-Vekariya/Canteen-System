using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Menu;

public class CreateMenuItemRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 9999.99)]
    public decimal Price { get; set; }

    public bool IsVeg { get; set; } = true;

    public bool IsAvailable { get; set; } = true;

    [MaxLength(50)]
    public string? Calories { get; set; }

    [Range(1, 999)]
    public int? PrepTimeMins { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(255)]
    public string? ImagePublicId { get; set; }
}
