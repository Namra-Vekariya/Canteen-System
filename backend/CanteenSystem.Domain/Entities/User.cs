using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Domain.Entities;

public class User : BaseEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? ProfileImagePublicId { get; set; }  // to delete the old images from cloudinary
    public bool IsActive { get; set; } = true;

    public DateTime? EmailVerifiedAt { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}