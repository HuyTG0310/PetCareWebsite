using FluentValidation;

namespace PetCareBooking.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
    {
        public CreateRoomCommandValidator()
        {
            RuleFor(x => x.RoomTypeId)
                .NotEmpty().WithMessage("Room type ID is required.");

            RuleFor(x => x.RoomName)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(50).WithMessage("Room name cannot exceed 50 characters.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid room status.")
                .When(x => x.Status.HasValue);
        }
    }
}
