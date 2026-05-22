namespace CanteenSystem.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required Guid UserId { get; set; } // FK → User.Id
    public required string TokenHash { get; set; }
    public required string Family { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public DateTime ExpiresAt { get; set; }

    public string? IpAddress { get; set; }            // Which IP issued this token
    public DateTime? RevokedAt { get; set; }          // When was it revoked
    public string? RevokedReason { get; set; }        // "Logout" | "PasswordReset" | "TokenReuse"
    // Navigation Property
    public virtual User? User { get; set; }
}