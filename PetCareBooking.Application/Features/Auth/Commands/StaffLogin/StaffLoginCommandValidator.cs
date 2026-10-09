using FluentValidation;

namespace PetCareBooking.Application.Features.Auth.Commands.StaffLogin
{
    public class StaffLoginCommandValidator : AbstractValidator<StaffLoginCommand>
    {
        public StaffLoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}