using System.Net;
using System.Text.RegularExpressions;
using CanteenSystem.Application.Common;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.DTOs.Menu;
using CanteenSystem.Application.Interfaces.Admin;
using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CanteenSystem.Application.Services.Admin;

public enum SlugType { Category, MenuItem }
public class AdminMenuService : IAdminMenuService
{
    private readonly IGenericRepository<Category> _categoryRepo;
    private readonly IGenericRepository<MenuItem> _menuItemRepo;

    public AdminMenuService(
        IGenericRepository<Category> categoryRepo,
        IGenericRepository<MenuItem> menuItemRepo)
    {
        _categoryRepo = categoryRepo;
        _menuItemRepo = menuItemRepo;
    }

    // ── Categories ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CategoryAdminResponse>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();

        return categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryAdminResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToList();
    }

    public async Task<CategoryAdminResponse> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new AppException("Category not found.", HttpStatusCode.NotFound);

        return MapCategoryToResponse(category);
    }

    public async Task<CategoryAdminResponse> CreateCategoryAsync(CreateCategoryRequest request, Guid userId)
    {
        var slug = GenerateSlug(request.Name);

        var existing = await _categoryRepo.GetFirstOrDefaultAsync(c => c.Slug == slug);
        if (existing != null)
            throw new AppException("A category with this name already exists.", HttpStatusCode.Conflict);

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedBy = userId
        };

        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();

        return MapCategoryToResponse(category);
    }

    public async Task<CategoryAdminResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, Guid userId)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new AppException("Category not found.", HttpStatusCode.NotFound);

        if (request.Name != null)
        {
            var newSlug = GenerateSlug(request.Name);
            var slugExists = await _categoryRepo.GetFirstOrDefaultAsync(c => c.Slug == newSlug && c.Id != id);
            if (slugExists != null)
                throw new AppException("A category with this name already exists.", HttpStatusCode.Conflict);

            category.Name = request.Name.Trim();
            category.Slug = newSlug;
        }

        if (request.Description != null)
            category.Description = request.Description.Trim();

        if (request.DisplayOrder.HasValue)
            category.DisplayOrder = request.DisplayOrder.Value;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        category.UpdatedBy = userId;

        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveChangesAsync();

        return MapCategoryToResponse(category);
    }

    public async Task DeleteCategoryAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new AppException("Category not found.", HttpStatusCode.NotFound);

        var hasItems = await _menuItemRepo.GetFirstOrDefaultAsync(m => m.CategoryId == id);
        if (hasItems != null)
            throw new AppException(
                "Cannot delete category with existing menu items. Remove all items first.",
                HttpStatusCode.BadRequest);

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = userId;

        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveChangesAsync();
    }

    public async Task<CategoryAdminResponse> ToggleCategoryActiveAsync(Guid id, Guid userId)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new AppException("Category not found.", HttpStatusCode.NotFound);

        category.IsActive = !category.IsActive;
        category.UpdatedBy = userId;

        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveChangesAsync();

        return MapCategoryToResponse(category);
    }

    public async Task ReorderCategoriesAsync(List<Guid> orderedIds)
    {
        if (orderedIds == null || orderedIds.Count == 0)
            throw new AppException("No category IDs provided.", HttpStatusCode.BadRequest);

        if (orderedIds.Distinct().Count() != orderedIds.Count)
            throw new AppException("Duplicate category IDs are not allowed.", HttpStatusCode.BadRequest);

        var categories = await _categoryRepo.GetAllAsync();
        var categoryDict = categories.ToDictionary(c => c.Id);

        var invalidIds = orderedIds.Where(id => !categoryDict.ContainsKey(id)).ToList();

        if (invalidIds.Any())
            throw new AppException(
                $"Category IDs not found: {string.Join(", ", invalidIds)}",
                HttpStatusCode.BadRequest);

        for (int i = 0; i < orderedIds.Count; i++)
        {
            if (categoryDict.TryGetValue(orderedIds[i], out var category))
            {
                category.DisplayOrder = i;
                await _categoryRepo.UpdateAsync(category);
            }
        }

        await _categoryRepo.SaveChangesAsync();
    }

    // ── Menu Items ──────────────────────────────────────────────────────────

    public async Task<PaginatedResponse<MenuItemAdminResponse>> GetMenuItemsAsync(PaginatedRequest request, Guid? categoryId = null)
    {
        var query = _menuItemRepo.GetAll()
            .Include(m => m.Category)
            .Where(m => !m.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(term) ||
                (m.Description != null && m.Description.ToLower().Contains(term)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
            "price" => request.IsDescending ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price),
            "category" => request.IsDescending ? query.OrderByDescending(m => m.Category!.Name) : query.OrderBy(m => m.Category!.Name),
            "created" => request.IsDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
            _ => query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await  query.CountAsync();


        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var responseItems = items.Select(m =>MapMenuItemToResponse(m, m.Category?.Name)).ToList();

        return PaginatedResponse<MenuItemAdminResponse>.Create(responseItems, totalCount, request);
    }

    public async Task<MenuItemAdminResponse> GetMenuItemByIdAsync(Guid id)
    {
        var item = await _menuItemRepo.GetByIdAsync(id)
            ?? throw new AppException("Menu item not found.", HttpStatusCode.NotFound);

        return MapMenuItemToResponse(item, item.Category?.Name);
    }

    public async Task<MenuItemAdminResponse> CreateMenuItemAsync(CreateMenuItemRequest request, Guid userId)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId)
            ?? throw new AppException("Category not found.", HttpStatusCode.NotFound);

        var slug = GenerateSlug(request.Name);
        var slugExists = await _menuItemRepo.GetFirstOrDefaultAsync(m => m.Slug == slug);
        if (slugExists != null)
            throw new AppException("A menu item with this name already exists.", HttpStatusCode.Conflict);

        var item = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            Price = request.Price,
            IsVeg = request.IsVeg,
            IsAvailable = request.IsAvailable,
            Calories = request.Calories?.Trim(),
            PrepTimeMins = request.PrepTimeMins,
            ImageUrl = request.ImageUrl,
            ImagePublicId = request.ImagePublicId,
            CreatedBy = userId
        };

        await _menuItemRepo.AddAsync(item);
        await _menuItemRepo.SaveChangesAsync();

        return MapMenuItemToResponse(item, category.Name);
    }

    public async Task<MenuItemAdminResponse> UpdateMenuItemAsync(Guid id, UpdateMenuItemRequest request, Guid userId)
    {
        var item = await _menuItemRepo.GetByIdAsync(id)
            ?? throw new AppException("Menu item not found.", HttpStatusCode.NotFound);

        if (request.Name != null)
        {
            var newSlug = GenerateSlug(request.Name);
            var slugExists = await _menuItemRepo.GetFirstOrDefaultAsync(m => m.Slug == newSlug && m.Id != id);
            if (slugExists != null)
                throw new AppException("A menu item with this name already exists.", HttpStatusCode.Conflict);

            item.Name = request.Name.Trim();
            item.Slug = newSlug;
        }

        if (request.CategoryId.HasValue)
            item.CategoryId = request.CategoryId.Value;

        if (request.Description != null)
            item.Description = request.Description.Trim();

        if (request.Price.HasValue)
            item.Price = request.Price.Value;

        if (request.IsVeg.HasValue)
            item.IsVeg = request.IsVeg.Value;

        if (request.IsAvailable.HasValue)
            item.IsAvailable = request.IsAvailable.Value;

        if (request.Calories != null)
            item.Calories = request.Calories.Trim();

        if (request.PrepTimeMins.HasValue)
            item.PrepTimeMins = request.PrepTimeMins.Value;

        if (request.ImageUrl != null)
            item.ImageUrl = request.ImageUrl;

        if (request.ImagePublicId != null)
            item.ImagePublicId = request.ImagePublicId;

        item.UpdatedBy = userId;

        await _menuItemRepo.UpdateAsync(item);
        await _menuItemRepo.SaveChangesAsync();

        return MapMenuItemToResponse(item, item.Category?.Name);
    }

    public async Task DeleteMenuItemAsync(Guid id, Guid userId)
    {
        var item = await _menuItemRepo.GetByIdAsync(id)
            ?? throw new AppException("Menu item not found.", HttpStatusCode.NotFound);

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;

        await _menuItemRepo.UpdateAsync(item);
        await _menuItemRepo.SaveChangesAsync();
    }

    public async Task<MenuItemAdminResponse> ToggleMenuItemAvailableAsync(Guid id, Guid userId)
    {
        var item = await _menuItemRepo.GetByIdAsync(id)
            ?? throw new AppException("Menu item not found.", HttpStatusCode.NotFound);

        item.IsAvailable = !item.IsAvailable;
        item.UpdatedBy = userId;

        await _menuItemRepo.UpdateAsync(item);
        await _menuItemRepo.SaveChangesAsync();

        return MapMenuItemToResponse(item, item.Category?.Name);
    }

    // ── Utility ─────────────────────────────────────────────────────────────

    public async Task<SlugCheckResponse> CheckSlugAsync(string slug, SlugType  type)
    {
        var sanitizedSlug = GenerateSlug(slug);
        bool isAvailable;

        if (type == SlugType.Category)
        {
            var existing = await _categoryRepo.GetFirstOrDefaultAsync(c => c.Slug == sanitizedSlug);
            isAvailable = existing == null;
        }
        else
        {
            var existing = await _menuItemRepo.GetFirstOrDefaultAsync(m => m.Slug == sanitizedSlug);
            isAvailable = existing == null;
        }

        return new SlugCheckResponse
        {
            IsAvailable = isAvailable,
            Slug = sanitizedSlug
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static readonly Regex SlugPattern = new(@"[^a-z0-9\s-]", RegexOptions.Compiled);

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower().Trim();
        slug = SlugPattern.Replace(slug, "");        // remove special chars
        slug = Regex.Replace(slug, @"\s+", "-");     // spaces to single dash
        slug = Regex.Replace(slug, @"-+", "-");      // collapse multiple dashes
        slug = slug.Trim('-');
        return slug;
    }

    private static CategoryAdminResponse MapCategoryToResponse(Category category)
    {
        return new CategoryAdminResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private static MenuItemAdminResponse MapMenuItemToResponse(MenuItem item, string? categoryName = null)
    {
        return new MenuItemAdminResponse
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Slug = item.Slug,
            Description = item.Description,
            Price = item.Price,
            ImageUrl = item.ImageUrl,
            IsVeg = item.IsVeg,
            IsAvailable = item.IsAvailable,
            Calories = item.Calories,
            PrepTimeMins = item.PrepTimeMins,
            CategoryName = categoryName ?? item.Category?.Name,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

}
