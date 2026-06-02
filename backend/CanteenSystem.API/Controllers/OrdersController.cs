using System.Security.Claims;
using CanteenSystem.Application.Common;
using CanteenSystem.Application.DTOs.Orders;
using CanteenSystem.Application.Interfaces.IOrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanteenSystem.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<CreateOrderResponse>>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(ApiResponse<CreateOrderResponse>.FailureResponse("Invalid or missing user token."));
            }

            var response = await _orderService.CreateOrderAsync(request, userId);

            return Ok(ApiResponse<CreateOrderResponse>.SuccessResponse(
                response,
                "Order placed successfully. Please present your token at the counter to pay."));
        }
    }
}
