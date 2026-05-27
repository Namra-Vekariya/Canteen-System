using CanteenSystem.Domain.Entities;

namespace CanteenSystem.Domain.Interfaces;

public interface IMenuRepository
{
     Task<IReadOnlyList<MenuItem>> GetMenuItemsWithCategoryAsync(bool isAvailable, Guid? categoryId = null);
}