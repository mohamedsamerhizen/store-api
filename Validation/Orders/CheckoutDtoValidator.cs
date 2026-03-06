using FluentValidation;
using store.Dtos.Orders;

namespace store.Validation.Orders
{
    public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
    {
        public CheckoutDtoValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Notes)
                .MaximumLength(1000);
        }
    }
}