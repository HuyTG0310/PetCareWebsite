using FluentValidation;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.UpdateRoomType
{
    public class UpdateRoomTypeCommandValidator : AbstractValidator<UpdateRoomTypeCommand>
    {
        public UpdateRoomTypeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room type ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Room type name is required.")
                .MaximumLength(100).WithMessage("Room type name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
