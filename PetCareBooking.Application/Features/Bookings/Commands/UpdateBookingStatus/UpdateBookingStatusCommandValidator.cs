using FluentValidation;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommandValidator : AbstractValidator<UpdateBookingStatusCommand>
    {
        public UpdateBookingStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Booking ID is required.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid booking status.");
        }
    }
}