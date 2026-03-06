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

        public List<OrderItemDto> Items { get; set; } = new();
    }
}