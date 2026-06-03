using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Menu;

public class UpdateMenuItemRequest
{
    public Guid? CategoryId { get; set; }

    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 9999.99)]
    public decimal? Price { get; set; }

    public bool? IsVeg { get; set; }

    public bool? IsAvailable { get; set; }

    [MaxLength(50)]
    public string? Calories { get; set; }

    [Range(1, 999)]
    public int? PrepTimeMins { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(255)]
    public string? ImagePublicId { get; set; }
}
