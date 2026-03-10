using store.Dtos.Orders;
using store.Models;

namespace store.Services.Orders
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(string userId, CheckoutRequest request);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<PagedOrdersDto> GetAllOrdersAsync(int page, int pageSize);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}