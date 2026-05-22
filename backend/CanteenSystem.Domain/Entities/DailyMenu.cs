using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class DailyMenu : BaseEntity
{
    public required Guid MenuItemId { get; set; }
    public required DateOnly MenuDate { get; set; }      // Date-only, no time component
    public required MealSlot MealSlot { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }                   // Admin note e.g., "Limited stock"

    // Max total orders allowed for this item today (null = unlimited
    public int? MaxQuantity { get; set; }

    /// Remaining orders left today (decremented on each order)
    public int AvailableQuantity { get; set; } = 0;

    public virtual MenuItem? MenuItem { get; set; }
}