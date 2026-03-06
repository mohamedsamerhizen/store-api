using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using store.Common;
using store.Data;
using store.Dtos.Cart;
using store.Models;
using System.Security.Claims;

namespace store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-cart")]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserId();

            var cart = await GetOrCreateCartAsync(userId);

            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Product)
                .LoadAsync();

            var response = BuildCartResponse(cart);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cart retrieved successfully.",
                Data = response
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = GetUserId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product is null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Product not found."
                });
            }

            if (!product.IsActive)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Product is not active."
                });
            }

            if (product.Stock < dto.Quantity)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Requested quantity exceeds stock."
                });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;

                if (newQuantity > product.Stock)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Total quantity exceeds available stock."
                    });
                }

                existingItem.Quantity = newQuantity;
                existingItem.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                _context.CartItems.Add(item);
            }

            await _context.SaveChangesAsync();

            var updatedCart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstAsync(c => c.UserId == userId);

            var response = BuildCartResponse(updatedCart);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product added to cart.",
                Data = response
            });
        }

        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, UpdateCartQuantityDto dto)
        {
            var userId = GetUserId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item is null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cart item not found."
                });
            }

            if (!item.Product.IsActive)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Product is not active."
                });
            }

            if (dto.Quantity > item.Product.Stock)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Quantity exceeds stock."
                });
            }

            item.Quantity = dto.Quantity;
            item.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstAsync(c => c.UserId == userId);

            var response = BuildCartResponse(cart);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Quantity updated.",
                Data = response
            });
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = GetUserId();

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item is null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cart item not found."
                });
            }

            _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Item removed from cart."
            });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cart already empty."
                });
            }

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cart cleared."
            });
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
                return cart;

            cart = new Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();

            return cart;
        }

        private object BuildCartResponse(Cart cart)
        {
            var items = cart.Items.Select(i => new
            {
                CartItemId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity,
                Total = i.Product.Price * i.Quantity
            });

            return new
            {
                CartId = cart.Id,
                Items = items,
                TotalAmount = items.Sum(x => x.Total)
            };
        }
    }
}