using FluentValidation;
using Store.DTOs.Orders;

namespace Store.Validation.Orders
{
    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(300);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        }
    }
}