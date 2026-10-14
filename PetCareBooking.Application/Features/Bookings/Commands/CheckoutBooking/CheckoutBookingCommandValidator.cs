using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking
{
    public class CheckoutBookingCommandValidator : AbstractValidator<CheckoutBookingCommand>
    {
        private static readonly string[] AllowedMethods = new[] { "CASH", "BANK_TRANSFER", "CARD", "VNPAY", "PREPAID" };

        public CheckoutBookingCommandValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.")
                .Must(m => AllowedMethods.Contains(m.Trim().ToUpper()))
                .WithMessage("Counter checkout accepts CASH, BANK_TRANSFER, CARD, or VNPAY/PREPAID.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).When(x => x.Amount.HasValue)
                .WithMessage("Payment amount must be greater than or equal to 0 if provided.");
        }
    }
}

