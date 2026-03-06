using Microsoft.EntityFrameworkCore;
using store.Data;
using store.Dtos.Orders;
using store.Models;

namespace store.Services.Orders;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto> CheckoutAsync(string userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        var cart = await _db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || !cart.Items.Any())
            throw new Exception("Cart is empty");

        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
                throw new Exception($"Product {item.Product.Name} is inactive");

            if (item.Product.Stock < item.Quantity)
                throw new Exception($"Not enough stock for {item.Product.Name}");
        }

        decimal subtotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
        decimal shipping = subtotal > 100 ? 0 : 10;
        decimal total = subtotal + shipping;

        var order = new Order
        {
            UserId = userId,
            Subtotal = subtotal,
            ShippingFee = shipping,
            Total = total,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var item in cart.Items)
        {
            item.Product.Stock -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Product.Price * item.Quantity
            });
        }

        _db.Orders.Add(order);

        _db.CartItems.RemoveRange(cart.Items);

        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return MapOrder(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task<PagedOrdersDto> GetAllOrdersAsync(int page, int pageSize)
    {
        var query = _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc);

        var total = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedOrdersDto
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Data = orders.Select(MapOrder).ToList()
        };
    }

    public async Task UpdateOrderStatusAsync(int id, OrderStatus newStatus)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            throw new Exception("Order not found");

        if (!IsValidTransition(order.Status, newStatus))
            throw new Exception($"Invalid transition {order.Status} → {newStatus}");

        if (newStatus == OrderStatus.Cancelled)
        {
            var productIds = order.Items.Select(i => i.ProductId).ToList();

            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                    product.Stock += item.Quantity;
            }
        }

        order.Status = newStatus;

        await _db.SaveChangesAsync();
    }

    private static bool IsValidTransition(OrderStatus current, OrderStatus next)
    {
        return current switch
        {
            OrderStatus.Pending => next == OrderStatus.Paid || next == OrderStatus.Cancelled,
            OrderStatus.Paid => next == OrderStatus.Shipped || next == OrderStatus.Cancelled,
            OrderStatus.Shipped => next == OrderStatus.Completed,
            _ => false
        };
    }

    private static OrderDto MapOrder(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CreatedAtUtc = order.CreatedAtUtc,
            Status = order.Status,
            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}