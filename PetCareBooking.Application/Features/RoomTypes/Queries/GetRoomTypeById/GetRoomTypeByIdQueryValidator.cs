using FluentValidation;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeById
{
    public class GetRoomTypeByIdQueryValidator : AbstractValidator<GetRoomTypeByIdQuery>
    {
        public GetRoomTypeByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Room type ID is required.");
        }
    }
}
