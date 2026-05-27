using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Interfaces;
using CanteenSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CanteenSystem.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly ApplicationDbContext _context;

    public MenuRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuItemsWithCategoryAsync(
        bool isAvailable, Guid? categoryId = null)
    {
        var query = _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.IsAvailable == isAvailable);

        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId.Value);

        return await query.ToListAsync(); 
    }
}