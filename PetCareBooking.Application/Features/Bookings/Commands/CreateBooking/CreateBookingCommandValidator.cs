using FluentValidation;
using PetCareBooking.Application.Features.Bookings.Commands.CreateBooking;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID cannot be empty.")
                .When(x => x.CustomerId.HasValue);

            RuleFor(x => x.BookingItems)
                .NotEmpty().WithMessage("Booking items are required.")
                .Must(items => items != null && items.Count > 0).WithMessage("At least one booking item is required.");

            RuleForEach(x => x.BookingItems).ChildRules(items =>
            {
                items.RuleFor(p => p.PetId)
                    .NotEmpty().WithMessage("Pet ID is required.");

                items.RuleFor(p => p.ServiceId)
                    .NotEmpty().WithMessage("Service ID is required.");

                items.RuleFor(p => p.ScheduledStartAt)
                    .NotEmpty().WithMessage("Scheduled start time is required.")
                    .GreaterThan(System.DateTime.UtcNow).WithMessage("Scheduled start time must be in the future.");

                items.RuleFor(p => p.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                    .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100.");

                items.RuleFor(p => p.ScheduledEndAt)
                    .GreaterThan(x => x.ScheduledStartAt)
                    .WithMessage("Scheduled end time must be after start time.")
                    .When(p => p.ScheduledEndAt.HasValue);
            });

            RuleFor(x => x.VoucherCode)
                .MaximumLength(50).WithMessage("Voucher code cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.VoucherCode));
        }
    }
}