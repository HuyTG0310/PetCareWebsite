using FluentValidation;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQueryValidator : AbstractValidator<GetRoomByIdQuery>
    {
        public GetRoomByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room ID is required.");
        }
    }
}
