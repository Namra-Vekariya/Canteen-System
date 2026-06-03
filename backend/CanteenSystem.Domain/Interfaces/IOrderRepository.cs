using CanteenSystem.Domain.Entities;

namespace CanteenSystem.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
}
