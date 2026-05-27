namespace CanteenSystem.Application.Interfaces;

using CanteenSystem.Application.DTOs.Menu;

public interface IMenuService
{
    Task<IReadOnlyList<CategoryResponse>> GetActiveCategoriesAsync();
    Task<IReadOnlyList<MenuItemResponse>> GetActiveMenuItemsAsync(Guid? categoryId = null);
}