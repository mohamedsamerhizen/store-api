using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Common;
using store.Dtos.Orders;
using store.Models;
using store.Services.Orders;

namespace store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var order = await _orderService.CheckoutAsync(userId, request);

            return StatusCode(201, ApiResponse.SuccessResponse("Order created successfully.", order));
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var orders = await _orderService.GetUserOrdersAsync(userId);

            return Ok(ApiResponse.SuccessResponse("Orders retrieved successfully.", orders));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllOrdersAsync(page, pageSize);

            return Ok(ApiResponse.SuccessResponse("Orders retrieved successfully.", result));
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromQuery] OrderStatus status)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, status);

            return Ok(ApiResponse.SuccessResponse("Order status updated successfully."));
        }
    }
}
