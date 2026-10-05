using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking
{
    public class CheckInBookingCommandValidator : AbstractValidator<CheckInBookingCommand>
    {
        public CheckInBookingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Booking ID is required.");
        }
    }
}
