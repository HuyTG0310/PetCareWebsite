using FluentValidation;

namespace PetCareBooking.Application.Features.Customers.Command.UpdateProfile
{
    public class UpdateCustomerProfileValidator : AbstractValidator<UpdateCustomerProfileCommand>
    {
        public UpdateCustomerProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Invalid Vietnamese phone number format.");

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters.");

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");
        }
    }
}