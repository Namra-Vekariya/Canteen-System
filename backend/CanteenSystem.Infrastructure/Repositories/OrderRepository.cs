using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Interfaces;
using CanteenSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CanteenSystem.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
