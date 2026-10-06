using FluentValidation;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.DeleteRoomType
{
    public class DeleteRoomTypeCommandValidator : AbstractValidator<DeleteRoomTypeCommand>
    {
        public DeleteRoomTypeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room type ID is required.");
        }
    }
}
