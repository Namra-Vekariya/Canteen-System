using CanteenSystem.Application.DTOs.Orders;

namespace CanteenSystem.Application.Interfaces.IOrderService
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequestDto request, Guid userId);
        Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId);
}
}