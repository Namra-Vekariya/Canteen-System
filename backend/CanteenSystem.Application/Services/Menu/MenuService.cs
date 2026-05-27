namespace CanteenSystem.Application.Services.Menu;

using CanteenSystem.Application.DTOs.Menu;
using CanteenSystem.Application.Interfaces;
using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Interfaces;

public class MenuService : IMenuService
{
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly IMenuRepository _menuRepo;

    public MenuService(
        IGenericRepository<Category> categoryRepo,
        IMenuRepository menuRepo)
    {
        _categoryRepo = categoryRepo;
        _menuRepo = menuRepo;
    }

   public async Task<IReadOnlyList<CategoryResponse>> GetActiveCategoriesAsync()
{
    var categories = await _categoryRepo.GetWhereAsync(c => c.IsActive);

    return categories
        .OrderBy(c => c.DisplayOrder)
        .Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder
        }).ToList().AsReadOnly();
}

  public async Task<IReadOnlyList<MenuItemResponse>> GetActiveMenuItemsAsync(Guid? categoryId = null)
{
    var items = await _menuRepo.GetMenuItemsWithCategoryAsync(isAvailable: true, categoryId);

    return items.Select(m => new MenuItemResponse
    {
        Id = m.Id,
        CategoryId = m.CategoryId,
        Name = m.Name,
        Slug = m.Slug,
        Description = m.Description,
        Price = m.Price,
        ImageUrl = m.ImageUrl,
        IsVeg = m.IsVeg,
        IsAvailable = m.IsAvailable,
        Calories = m.Calories,
        PrepTimeMins = m.PrepTimeMins,
        CategoryName = m.Category?.Name
    }).ToList().AsReadOnly();
}
}