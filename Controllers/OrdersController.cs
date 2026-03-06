using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Common;
using store.Models;
using store.Services.Orders;
using System.Security.Claims;

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
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var order = await _orderService.CheckoutAsync(userId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Order created successfully",
                Data = order
            });
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var orders = await _orderService.GetUserOrdersAsync(userId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Data = orders
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(
            int page = 1,
            int pageSize = 10)
        {
            var result = await _orderService.GetAllOrdersAsync(page, pageSize);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Data = result
            });
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            [FromQuery] OrderStatus status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, status);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Order status updated"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}