using Microsoft.EntityFrameworkCore;
using store.Data;
using store.Dtos.Cart;
using store.Models;

namespace store.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetMyCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.Product)
                .LoadAsync();

            return BuildCartResponse(cart);
        }

        public async Task<object> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException("Product is not active.");
            }

            if (product.Stock < dto.Quantity)
            {
                throw new InvalidOperationException("Requested quantity exceeds stock.");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Models.Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem is not null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;

                if (newQuantity > product.Stock)
                {
                    throw new InvalidOperationException("Total quantity exceeds available stock.");
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

            return BuildCartResponse(updatedCart);
        }

        public async Task<object> UpdateQuantityAsync(string userId, int cartItemId, UpdateCartQuantityDto dto)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item is null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }

            if (!item.Product.IsActive)
            {
                throw new InvalidOperationException("Product is not active.");
            }

            if (dto.Quantity > item.Product.Stock)
            {
                throw new InvalidOperationException("Quantity exceeds stock.");
            }

            item.Quantity = dto.Quantity;
            item.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstAsync(c => c.UserId == userId);

            return BuildCartResponse(cart);
        }

        public async Task RemoveItemAsync(string userId, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item is null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null || !cart.Items.Any())
            {
                return false;
            }

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<Models.Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is not null)
            {
                return cart;
            }

            cart = new Models.Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return cart;
        }

        private static object BuildCartResponse(Models.Cart cart)
        {
            var items = cart.Items.Select(i => new
            {
                CartItemId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity,
                Total = i.Product.Price * i.Quantity
            }).ToList();

            return new
            {
                CartId = cart.Id,
                Items = items,
                TotalAmount = items.Sum(x => x.Total)
            };
        }
    }
}