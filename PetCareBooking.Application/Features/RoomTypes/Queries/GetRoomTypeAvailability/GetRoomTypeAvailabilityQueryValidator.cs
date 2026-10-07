using FluentValidation;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeAvailability
{
    public class GetRoomTypeAvailabilityQueryValidator : AbstractValidator<GetRoomTypeAvailabilityQuery>
    {
        public GetRoomTypeAvailabilityQueryValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty().WithMessage("Start date (From) is required.");

            RuleFor(x => x.To)
                .NotEmpty().WithMessage("End date (To) is required.")
                .GreaterThan(x => x.From).WithMessage("End date (To) must be strictly greater than Start date (From).");
        }
    }
}
