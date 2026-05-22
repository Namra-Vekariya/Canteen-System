using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

/// <summary>
/// Immutable audit trail — every status change on an order is logged here.
/// Used as the source of truth for SignalR real-time updates.
/// Never soft-delete this table (no global query filter applied).
/// </summary>
public class OrderStatusLog : BaseEntity
{
    public required Guid OrderId { get; set; }

    // Null only for the very first status log (initial Pending)
    public OrderStatus? FromStatus { get; set; }
    public required OrderStatus ToStatus { get; set; }

    // UserId of whoever triggered this change (user or admin)
    public required Guid ChangedBy { get; set; }

    public string? Note { get; set; }                    // Optional admin note

    public virtual Order? Order { get; set; }
    public virtual User? ChangedByUser { get; set; }
}