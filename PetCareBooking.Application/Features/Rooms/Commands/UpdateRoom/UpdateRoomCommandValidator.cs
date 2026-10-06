using FluentValidation;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
    {
        public UpdateRoomCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room ID is required.");

            RuleFor(x => x.RoomTypeId)
                .NotEmpty().WithMessage("Room type ID is required.");

            RuleFor(x => x.RoomName)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(50).WithMessage("Room name cannot exceed 50 characters.");
        }
    }
}
