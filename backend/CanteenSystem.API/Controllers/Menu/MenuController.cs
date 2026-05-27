using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using CanteenSystem.Application.DTOs.Menu;       
using CanteenSystem.Application.Common;
using CanteenSystem.Application.Interfaces;          

namespace CanteenSystem.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize] // Keeps the menu secure so only logged-in users can fetch it
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    // GET: api/v1/Menu/categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _menuService.GetActiveCategoriesAsync();

        return Ok(
            ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse(
                categories, 
                "Categories fetched successfully"
            )
        );
    }

    // GET: api/v1/Menu/items
    // GET: api/v1/Menu/items?categoryId={guid}
    [HttpGet("items")]
    public async Task<IActionResult> GetMenuItems([FromQuery] Guid? categoryId)
    {
        var items = await _menuService.GetActiveMenuItemsAsync(categoryId);

        return Ok(
            ApiResponse<IEnumerable<MenuItemResponse>>.SuccessResponse(
                items, 
                "Menu items fetched successfully"
            )
        );
    }
}