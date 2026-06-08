using System.Net;
using System.Security.Claims;
using CanteenSystem.Application.Common;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.DTOs.Menu;
using CanteenSystem.Application.Interfaces.Admin;
using CanteenSystem.Application.Services.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CanteenSystem.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/menu")]
public class MenuController : ControllerBase
{
    private readonly IAdminMenuService _adminMenuService;

    public MenuController(IAdminMenuService adminMenuService)
    {
        _adminMenuService = adminMenuService;
    }

    // ── Categories ──────────────────────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryAdminResponse>>>> GetCategories()
    {
        var categories = await _adminMenuService.GetCategoriesAsync();
        return Ok(ApiResponse<IReadOnlyList<CategoryAdminResponse>>.SuccessResponse(categories));
    }


    [HttpGet("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryAdminResponse>>> GetCategory(Guid id)
    {
        var category = await _adminMenuService.GetCategoryByIdAsync(id);
        return Ok(ApiResponse<CategoryAdminResponse>.SuccessResponse(category));
    }

    // [CHANGED] Returns 201 Created with location header instead of 200 OK — correct REST standard for resource creation
    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<CategoryAdminResponse>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await _adminMenuService.CreateCategoryAsync(request, userId);
        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            ApiResponse<CategoryAdminResponse>.SuccessResponse(category, "Category created successfully."));
    }

    [HttpPatch("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryAdminResponse>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await _adminMenuService.UpdateCategoryAsync(id, request, userId);
        return Ok(ApiResponse<CategoryAdminResponse>.SuccessResponse(category, "Category updated successfully."));
    }

    // [CHANGED] Return null instead of new() as delete response data — empty object was noise
    [HttpDelete("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
    {
        var userId = GetUserId();
        await _adminMenuService.DeleteCategoryAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse("Category deleted successfully."));
    }

    [HttpPatch("categories/{id:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<CategoryAdminResponse>>> ToggleCategoryActive(Guid id)
    {
        var userId = GetUserId();
        var category = await _adminMenuService.ToggleCategoryActiveAsync(id, userId);
        return Ok(ApiResponse<CategoryAdminResponse>.SuccessResponse(category));
    }

    // [CHANGED] Added null/empty guard at controller level before hitting service — defense in depth
    [HttpPut("categories/reorder")]
    public async Task<ActionResult<ApiResponse<object>>> ReorderCategories([FromBody] List<Guid> orderedIds)
    {
        if (orderedIds == null || orderedIds.Count == 0)
            return BadRequest(ApiResponse<object>.FailureResponse("No category IDs provided."));

        await _adminMenuService.ReorderCategoriesAsync(orderedIds);
        return Ok(ApiResponse<object>.SuccessResponse("Categories reordered successfully."));
    }

    // ── Menu Items ──────────────────────────────────────────────────────────

    // [CHANGED] Replaced 5 individual [FromQuery] params with [FromQuery] PaginatedRequest
    // Framework binds query params directly to the object — no manual mapping needed
    // Adding new filter fields now only requires changing PaginatedRequest, not controller signature
    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<MenuItemAdminResponse>>>> GetMenuItems(
        [FromQuery] PaginatedRequest request,
        [FromQuery] Guid? categoryId = null)
    {
        var items = await _adminMenuService.GetMenuItemsAsync(request, categoryId);
        return Ok(ApiResponse<PaginatedResponse<MenuItemAdminResponse>>.SuccessResponse(items));
    }

    [HttpGet("items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<MenuItemAdminResponse>>> GetMenuItem(Guid id)
    {
        var item = await _adminMenuService.GetMenuItemByIdAsync(id);
        return Ok(ApiResponse<MenuItemAdminResponse>.SuccessResponse(item));
    }

    // [CHANGED] Returns 201 Created with location header instead of 200 OK — correct REST standard for resource creation
    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<MenuItemAdminResponse>>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var userId = GetUserId();
        var item = await _adminMenuService.CreateMenuItemAsync(request, userId);
        return CreatedAtAction(
            nameof(GetMenuItem),
            new { id = item.Id },
            ApiResponse<MenuItemAdminResponse>.SuccessResponse(item, "Menu item created successfully."));
    }

    [HttpPatch("items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<MenuItemAdminResponse>>> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request)
    {
        var userId = GetUserId();
        var item = await _adminMenuService.UpdateMenuItemAsync(id, request, userId);
        return Ok(ApiResponse<MenuItemAdminResponse>.SuccessResponse(item, "Menu item updated successfully."));
    }

    // [CHANGED] Return null instead of new() as delete response data — empty object was noise
    [HttpDelete("items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMenuItem(Guid id)
    {
        var userId = GetUserId();
        await _adminMenuService.DeleteMenuItemAsync(id, userId);
        return Ok(ApiResponse<object>.SuccessResponse("Menu item deleted successfully."));
    }

    [HttpPatch("items/{id:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<MenuItemAdminResponse>>> ToggleMenuItemAvailable(Guid id)
    {
        var userId = GetUserId();
        var item = await _adminMenuService.ToggleMenuItemAvailableAsync(id, userId);
        return Ok(ApiResponse<MenuItemAdminResponse>.SuccessResponse(item));
    }

    // ── Utility ─────────────────────────────────────────────────────────────

    // [CHANGED] type parameter changed from string to SlugType enum
    // ASP.NET Core automatically parses ?type=Category or ?type=0 into the enum
    [HttpGet("check-slug")]
    public async Task<ActionResult<ApiResponse<SlugCheckResponse>>> CheckSlug(
        [FromQuery] string slug,
        [FromQuery] SlugType type)
    {
        var result = await _adminMenuService.CheckSlugAsync(slug, type);
        return Ok(ApiResponse<SlugCheckResponse>.SuccessResponse(result));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    // [CHANGED] Replaced UnauthorizedAccessException with AppException(HttpStatusCode.Unauthorized)
    // UnauthorizedAccessException was not caught by global exception handler — returned 500 instead of 401
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            throw new AppException("Invalid or missing user token.", HttpStatusCode.Unauthorized);

        return userId;
    }
}