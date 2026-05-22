using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class Otp : BaseEntity
{
    public required Guid UserId { get; set; }

    // EmailVerification or PasswordReset
    public required OtpType Type { get; set; }

    // SHA-256 hash of the 6-digit OTP code 
    public required string OtpHash { get; set; }

    public required DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    /// <summary>Count of wrong attempts — lock after max (e.g., 5)</summary>
    public int Attempts { get; set; } = 0;

    // ── Navigation Property ───────────────────────────────────────────────
    public virtual User? User { get; set; }
}