using FluentValidation;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetAvailableRooms
{
    public class GetAvailableRoomsQueryValidator : AbstractValidator<GetAvailableRoomsQuery>
    {
        public GetAvailableRoomsQueryValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty().WithMessage("Start date (From) is required.");

            RuleFor(x => x.To)
                .NotEmpty().WithMessage("End date (To) is required.")
                .GreaterThan(x => x.From).WithMessage("End date (To) must be strictly greater than Start date (From).");
        }
    }
}
