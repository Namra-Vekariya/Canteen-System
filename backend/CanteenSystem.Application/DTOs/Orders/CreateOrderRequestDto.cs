using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Orders
{
    public class CreateOrderRequestDto
    {
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<OrderItemRequestDto> Items { get; set; } = new List<OrderItemRequestDto>();

        public class OrderItemRequestDto
        {
            [Required]
            public Guid MenuItemId { get; set; }

            [Required]
            [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
            public int Quantity { get; set; }
        }
    }
}
