using FluentValidation;
using store.Dtos.Orders;

namespace store.Validation.Orders
{
    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .MaximumLength(300);

            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }
    }
}