using CanteenSystem.Application.Common;
using CanteenSystem.Application.DTOs.Menu;
using CanteenSystem.Application.Services.Admin;

namespace CanteenSystem.Application.Interfaces.Admin;

public interface IAdminMenuService
{
    // ── Categories ──────────────────────────────────────────
    Task<IReadOnlyList<CategoryAdminResponse>> GetCategoriesAsync();
    Task<CategoryAdminResponse> GetCategoryByIdAsync(Guid id);
    Task<CategoryAdminResponse> CreateCategoryAsync(CreateCategoryRequest request, Guid userId);
    Task<CategoryAdminResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, Guid userId);
    Task DeleteCategoryAsync(Guid id, Guid userId);
    Task<CategoryAdminResponse> ToggleCategoryActiveAsync(Guid id, Guid userId);
    Task ReorderCategoriesAsync(List<Guid> orderedIds);

    // ── Menu Items ──────────────────────────────────────────
    Task<PaginatedResponse<MenuItemAdminResponse>> GetMenuItemsAsync(PaginatedRequest request, Guid? categoryId = null);
    Task<MenuItemAdminResponse> GetMenuItemByIdAsync(Guid id);
    Task<MenuItemAdminResponse> CreateMenuItemAsync(CreateMenuItemRequest request, Guid userId);
    Task<MenuItemAdminResponse> UpdateMenuItemAsync(Guid id, UpdateMenuItemRequest request, Guid userId);
    Task DeleteMenuItemAsync(Guid id, Guid userId);
    Task<MenuItemAdminResponse> ToggleMenuItemAvailableAsync(Guid id, Guid userId);

    // ── Utility ─────────────────────────────────────────────
    Task<SlugCheckResponse> CheckSlugAsync(string slug, SlugType type);
}
