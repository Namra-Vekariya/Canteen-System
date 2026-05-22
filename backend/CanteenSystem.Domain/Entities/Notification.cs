using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class Notification : BaseEntity
{
    public required Guid UserId { get; set; }
    public required NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }

    /// <summary>
    /// JSON payload for extra context — stored as PostgreSQL jsonb.
    /// Example: { "orderId": "...", "orderToken": "ORD-XK9F2", "status": "Ready" }
    /// </summary>
    public string? Data { get; set; } //?? jsonb

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // ── Navigation Property ───────────────────────────────────────────────
    public virtual User? User { get; set; }
}