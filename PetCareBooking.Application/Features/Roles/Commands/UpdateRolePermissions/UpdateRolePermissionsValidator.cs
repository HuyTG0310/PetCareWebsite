using FluentValidation;

namespace PetCareBooking.Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsCommand>
    {
        public UpdateRolePermissionsValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.");

            RuleFor(x => x.PermissionIds)
                .NotNull().WithMessage("Permission IDs list must not be null.");
        }
    }
}
