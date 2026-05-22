namespace CanteenSystem.Domain.Entities;

public class Cart : BaseEntity
{
    public required Guid UserId { get; set; }

    // Cart auto-expires after inactivity 
    public DateTime? ExpiresAt { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}