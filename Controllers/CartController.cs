using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Common;
using store.Dtos.Cart;
using store.Services.Cart;
using System.Security.Claims;

namespace store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var response = await _cartService.GetMyCartAsync(userId);

            return Ok(ApiResponse.SuccessResponse("Cart retrieved successfully.", response));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var response = await _cartService.AddToCartAsync(userId, dto);

            return Ok(ApiResponse.SuccessResponse("Product added to cart.", response));
        }

        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartQuantityDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var response = await _cartService.UpdateQuantityAsync(userId, cartItemId, dto);

            return Ok(ApiResponse.SuccessResponse("Quantity updated.", response));
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            await _cartService.RemoveItemAsync(userId, cartItemId);

            return Ok(ApiResponse.SuccessResponse("Item removed from cart."));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse.FailResponse("User not authenticated."));
            }

            var cleared = await _cartService.ClearCartAsync(userId);

            if (!cleared)
            {
                return Ok(ApiResponse.SuccessResponse("Cart already empty."));
            }

            return Ok(ApiResponse.SuccessResponse("Cart cleared."));
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}