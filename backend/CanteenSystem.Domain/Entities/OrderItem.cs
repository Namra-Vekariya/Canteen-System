namespace CanteenSystem.Domain.Entities;

public class OrderItem : BaseEntity
{
    public required Guid OrderId { get; set; }
    public Guid? MenuItemId { get; set; }

    // ── Snapshot at time of order (immutable record) ──────────────────────
    public required string ItemName { get; set; }
    public string? ItemImageUrl { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Quantity { get; set; }
    public required decimal LineTotal { get; set; }      // UnitPrice × Quantity
    public required bool IsVeg { get; set; }

    public virtual Order? Order { get; set; }
    public virtual MenuItem? MenuItem { get; set; }
}