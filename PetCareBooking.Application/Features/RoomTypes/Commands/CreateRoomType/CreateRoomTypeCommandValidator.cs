using FluentValidation;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.CreateRoomType
{
    public class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
    {
        public CreateRoomTypeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Room type name is required.")
                .MaximumLength(100).WithMessage("Room type name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
