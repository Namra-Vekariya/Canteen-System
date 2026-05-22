using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class Order : BaseEntity
{
    public required Guid UserId { get; set; }

    // Short alphanumeric pickup token shown at counter (e.g., "ORD-XK9F2")
    public required string Token { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public required decimal TotalAmount { get; set; }     

    // ── Order Details ──────────────────────────────────────────────────────
    public string? SpecialInstructions { get; set; }

    // ── Cancellation ───────────────────────────────────────────────────────
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // ── Collection ─────────────────────────────────────────────────────────
    public DateTime? CollectedAt { get; set; }

    public virtual User? User { get; set; }
    public virtual Payment? Payment { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusLog> StatusLogs { get; set; } = new List<OrderStatusLog>();
}