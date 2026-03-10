using store.Dtos.Cart;

namespace store.Services.Cart
{
    public interface ICartService
    {
        Task<object> GetMyCartAsync(string userId);
        Task<object> AddToCartAsync(string userId, AddToCartDto dto);
        Task<object> UpdateQuantityAsync(string userId, int cartItemId, UpdateCartQuantityDto dto);
        Task RemoveItemAsync(string userId, int cartItemId);
        Task<bool> ClearCartAsync(string userId);
    }
}