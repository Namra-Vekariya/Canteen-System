using System.Net;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.DTOs.Orders;
using CanteenSystem.Application.Interfaces.IOrderService;
using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using CanteenSystem.Domain.Interfaces;

namespace CanteenSystem.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IOrderRepository _orderDetailRepository;
        private readonly IGenericRepository<MenuItem> _menuItemRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<OrderStatusLog> _statusLogRepository;

        public OrderService(
            IGenericRepository<Order> orderRepository,
            IOrderRepository orderDetailRepository,
            IGenericRepository<MenuItem> menuItemRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<OrderStatusLog> statusLogRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _menuItemRepository = menuItemRepository;
            _paymentRepository = paymentRepository;
            _statusLogRepository = statusLogRepository;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequestDto request, Guid userId)
        {
            var requestedItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
            var dbMenuItems = (await _menuItemRepository.GetWhereAsync(m => requestedItemIds.Contains(m.Id))).ToList();

            var missingIds = requestedItemIds.Except(dbMenuItems.Select(m => m.Id)).ToList();
            if (missingIds.Any())
            {
                throw new AppException(
                    "One or more items in your cart do not exist.",
                    HttpStatusCode.NotFound,
                    missingIds.Select(id => $"MenuItemId: {id}").ToList());
            }

            var unavailableItem = dbMenuItems.FirstOrDefault(m => !m.IsAvailable);
            if (unavailableItem != null)
            {
                throw new AppException(
                    $"Sorry, {unavailableItem.Name} is currently sold out.",
                    HttpStatusCode.BadRequest);
            }

            var token = await GenerateUniqueOrderTokenAsync();

            var order = new Order
            {
                UserId = userId,
                Token = token,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                TotalAmount = 0,
                OrderItems = new List<OrderItem>()
            };

            decimal grandTotal = 0m;
            var responseItems = new List<OrderItemDto>();

            foreach (var requestItem in request.Items)
            {
                var dbItem = dbMenuItems.First(m => m.Id == requestItem.MenuItemId);
                var lineTotal = dbItem.Price * requestItem.Quantity;
                grandTotal += lineTotal;

                order.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = dbItem.Id,
                    ItemName = dbItem.Name,
                    ItemImageUrl = dbItem.ImageUrl,
                    UnitPrice = dbItem.Price,
                    Quantity = requestItem.Quantity,
                    LineTotal = lineTotal,
                    IsVeg = dbItem.IsVeg
                });

                responseItems.Add(new OrderItemDto
                {
                    ItemName = dbItem.Name,
                    ImageUrl = dbItem.ImageUrl,
                    Quantity = requestItem.Quantity,
                    UnitPrice = dbItem.Price
                });
            }

            order.TotalAmount = grandTotal;

            order.Payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                Amount = grandTotal,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Pending
            };

            order.StatusLogs.Add(new OrderStatusLog
            {
                OrderId = order.Id,
                FromStatus = null,
                ToStatus = OrderStatus.Pending,
                ChangedBy = userId,
                Note = "Order created, preparation will start once the order is accepted."
            });

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return new CreateOrderResponse
            {
                OrderId = order.Id,
                OrderNumber = order.Token,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = PaymentMethod.Cash,
                Items = responseItems
            };
        }

        private async Task<string> GenerateUniqueOrderTokenAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            while (true)
            {
                var randomString = new string(Enumerable.Repeat(chars, 5)
                    .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

                var token = $"ORD-{randomString}";

                var existing = await _orderRepository.GetFirstOrDefaultAsync(o => o.Token == token);
                if (existing == null)
                {
                    return token;
                }
            }
        }
        public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId, Guid userId)
        {
            var order = await _orderDetailRepository.GetOrderWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new AppException("Order not found.", HttpStatusCode.NotFound);
            }
            if (order.UserId != userId)
            {
                throw new AppException("You are not authorized to view this order.", HttpStatusCode.Forbidden);
            }
            return new OrderResponse
            {
                OrderId = order.Id,
                OrderNumber = order.Token,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                PaymentMethod = order.Payment?.Method.ToString() ?? "Cash",
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                CollectedAt = order.CollectedAt,
                CancelledAt = order.CancelledAt,
                SpecialInstructions = order.SpecialInstructions,
                Items = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    ItemId = oi.Id,
                    ItemName = oi.ItemName,
                    ImageUrl = oi.ItemImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.LineTotal,
                    IsVeg = oi.IsVeg
                }).ToList()
            };
        }
    }
}
