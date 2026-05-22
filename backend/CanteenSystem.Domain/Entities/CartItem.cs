namespace CanteenSystem.Domain.Entities;

public class CartItem : BaseEntity
{
    public required Guid CartId { get; set; }
    public required Guid MenuItemId { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public virtual Cart? Cart { get; set; }
    public virtual MenuItem? MenuItem { get; set; }
}