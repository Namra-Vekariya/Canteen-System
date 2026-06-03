using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Application.DTOs.Orders
{
    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
        public required string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // Returning the finalized items so the frontend confirmation page can display them
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

public class OrderItemDto
{
    public required string ItemName { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
}
