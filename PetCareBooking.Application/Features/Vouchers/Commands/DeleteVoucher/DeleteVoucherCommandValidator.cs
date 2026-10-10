using FluentValidation;

namespace PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher
{
    public class DeleteVoucherCommandValidator : AbstractValidator<DeleteVoucherCommand>
    {
        public DeleteVoucherCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Voucher ID is required.");
        }
    }
}
