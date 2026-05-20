namespace CanteenSystem.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required Guid UserId { get; set; } // FK → User.Id
    public required string TokenHash { get; set; }
    public required string Family { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public DateTime ExpiresAt { get; set; }

    // Navigation Property
    public virtual User? User { get; set; }
}