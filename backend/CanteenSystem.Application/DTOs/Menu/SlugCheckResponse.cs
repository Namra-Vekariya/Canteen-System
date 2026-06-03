namespace CanteenSystem.Application.DTOs.Menu;

public class SlugCheckResponse
{
    public bool IsAvailable { get; set; }
    public string Slug { get; set; } = string.Empty;
}
