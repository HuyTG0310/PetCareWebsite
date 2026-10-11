using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Commands.DepositBooking
{
    public class DepositBookingCommandValidator : AbstractValidator<DepositBookingCommand>
    {
        private static readonly string[] AllowedMethods = new[] { "CASH", "BANK_TRANSFER", "CARD", "E_WALLET", "VNPAY" };

        public DepositBookingCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Deposit amount must be greater than 0.");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.")
                .Must(m => AllowedMethods.Contains(m.Trim().ToUpper()))
                .WithMessage($"Payment method must be one of: {string.Join(", ", AllowedMethods)}.");
        }
    }
}

