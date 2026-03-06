using System;
using System.Collections.Generic;
using store.Models;
namespace store.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }

        // Shipping Info
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string AddressLine1 { get; set; } = default!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = default!;
        public string? Notes { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}