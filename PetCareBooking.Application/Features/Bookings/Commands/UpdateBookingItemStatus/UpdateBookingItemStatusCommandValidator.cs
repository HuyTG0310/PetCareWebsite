using FluentValidation;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus
{
    public class UpdateBookingItemStatusCommandValidator : AbstractValidator<UpdateBookingItemStatusCommand>
    {
        public UpdateBookingItemStatusCommandValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty().WithMessage("Booking ID is required.");

            RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid booking item status.");

            RuleFor(x => x.ResultNote)
                .MaximumLength(1000).WithMessage("Result note cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ResultNote));

            RuleFor(x => x.ResultImageUrl)
                .MaximumLength(500).WithMessage("Result image URL cannot exceed 500 characters.")
                .Matches(@"^https?://").WithMessage("Result image URL must start with http:// or https://")
                .When(x => !string.IsNullOrEmpty(x.ResultImageUrl));

            // Validate status-specific requirements
            RuleFor(x => x.ResultNote)
                .NotEmpty().WithMessage("Result note is required when status is Completed.")
                .When(x => x.Status == BookingItemStatus.Completed);

            RuleFor(x => x.StaffId)
                .NotEmpty().WithMessage("Staff ID is required when status is Checked_In or In_Progress.")
                .When(x => x.Status == BookingItemStatus.Checked_In || x.Status == BookingItemStatus.In_Progress);
        }
    }
}