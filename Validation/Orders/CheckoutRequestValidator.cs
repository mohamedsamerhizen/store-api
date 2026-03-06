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
                .WithMessage("Full name is required.")
                .MaximumLength(200);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .WithMessage("Address is required.")
                .MaximumLength(300);

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required.")
                .MaximumLength(150);

            RuleFor(x => x.AddressLine2)
                .MaximumLength(300);

            RuleFor(x => x.Notes)
                .MaximumLength(1000);
        }
    }
}