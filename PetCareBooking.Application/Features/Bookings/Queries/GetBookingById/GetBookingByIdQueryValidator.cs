using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Booking ID is required.");
        }
    }
}