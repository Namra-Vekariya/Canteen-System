using CanteenSystem.Domain.Interfaces;

namespace CanteenSystem.Domain.Entities;

public abstract class BaseEntity : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // IAuditable
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}