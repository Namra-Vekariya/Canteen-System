namespace CanteenSystem.Application.Common;

public class PaginatedRequest
{
    private const int MaxPageSize = 50;
     private const int DefaultPageSize = 10;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
         set => _pageSize = value < 1 ? DefaultPageSize : value > MaxPageSize ? MaxPageSize : value;        
    }

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
}
