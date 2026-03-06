using FluentValidation;
using store.Dtos.Cart;

namespace store.Validation.Cart
{
    public class UpdateCartQuantityDtoValidator : AbstractValidator<UpdateCartQuantityDto>
    {
        public UpdateCartQuantityDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");
        }
    }
}