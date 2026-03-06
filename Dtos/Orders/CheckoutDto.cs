namespace store.Dtos.Orders
{
    public class CheckoutDto
    {
        public string ShippingAddress { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}