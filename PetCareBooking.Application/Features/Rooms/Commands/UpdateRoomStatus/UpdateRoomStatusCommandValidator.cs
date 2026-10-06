using FluentValidation;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoomStatus
{
    public class UpdateRoomStatusCommandValidator : AbstractValidator<UpdateRoomStatusCommand>
    {
        public UpdateRoomStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room ID is required.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid room status. Allowed values are: Available, Maintenance.");
        }
    }
}
