using FluentValidation;

namespace PetCareBooking.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
    {
        public UpdateStaffCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters.");

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid staff status.");

            RuleFor(x => x.RoleIds)
                .NotEmpty().WithMessage("At least one role must be assigned.");

            RuleForEach(x => x.RoleIds)
                .NotEmpty().WithMessage("Role ID cannot be empty.");
        }
    }
}