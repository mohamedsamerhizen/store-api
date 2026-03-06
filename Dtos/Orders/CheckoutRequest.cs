namespace store.Dtos.Orders
{
    public class CheckoutRequest
    {
        public string FullName { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        public string AddressLine1 { get; set; } = default!;

        public string? AddressLine2 { get; set; }

        public string City { get; set; } = default!;

        public string? Notes { get; set; }
    }
}