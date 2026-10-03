using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Booking ID is required.");

            RuleFor(x => x.CancellationReason)
                .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.CancellationReason));
        }
    }
}