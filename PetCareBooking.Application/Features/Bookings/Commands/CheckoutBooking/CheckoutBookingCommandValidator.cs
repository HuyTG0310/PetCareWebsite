using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking
{
    public class CheckoutBookingCommandValidator : AbstractValidator<CheckoutBookingCommand>
    {
        private static readonly string[] AllowedMethods = new[] { "CASH", "BANK_TRANSFER" };

        public CheckoutBookingCommandValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.")
                .Must(m => AllowedMethods.Contains(m.Trim().ToUpper()))
                .WithMessage("Counter checkout only accepts CASH (Tiền mặt) or BANK_TRANSFER (Chuyển khoản VietQR).");

            RuleFor(x => x.Amount)
                .GreaterThan(0).When(x => x.Amount.HasValue)
                .WithMessage("Payment amount must be greater than 0 if provided.");
        }
    }
}

