using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class Payment : BaseEntity
{
    public required Guid OrderId { get; set; }
    public required Guid UserId { get; set; }
    public required decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    public string? Gateway { get; set; }                 // e.g., "Razorpay", "PayU"
    public string? GatewayTxnId { get; set; }
    public string? GatewayOrderId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }

    // ── Refund ────────────────────────────────────────────────────────────
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }

    // ── Navigation Properties ─────────────────────────────────────────────
    public virtual Order? Order { get; set; }
    public virtual User? User { get; set; }
}