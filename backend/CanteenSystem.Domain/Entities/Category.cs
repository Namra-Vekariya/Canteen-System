namespace CanteenSystem.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }            //  "Breakfast"
    public required string Slug { get; set; }            //  "breakfast"
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;          
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ─────────────────────────────────────────────
    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}