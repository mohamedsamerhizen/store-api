using System;
using System.Collections.Generic;

namespace store.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // Identity User Id (string)
        public string UserId { get; set; } = default!;

        public ApplicationUser User { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public List<CartItem> Items { get; set; } = new();
    }
}