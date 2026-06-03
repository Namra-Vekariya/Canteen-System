using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Menu;

public class UpdateCategoryRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 1000)]
    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }
}
