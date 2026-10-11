using FluentValidation;

namespace PetCareBooking.Application.Features.Staffs.Commands.ChangeStaffPassword
{
    public class ChangeStaffPasswordCommandValidator : AbstractValidator<ChangeStaffPasswordCommand>
    {
        public ChangeStaffPasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm new password is required.")
                .Equal(x => x.NewPassword).WithMessage("Confirm password does not match the new password.");
        }
    }
}
