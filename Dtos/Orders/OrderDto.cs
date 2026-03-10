using System;
using System.Collections.Generic;
using store.Models;

namespace store.Dtos.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();
    }
}