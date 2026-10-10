using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher
{
    public class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
    {
        public UpdateVoucherCommandValidator(IGenericRepository<Voucher>? voucherRepository = null)
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("Voucher ID is required.");

            RuleFor(v => v.Code)
                .NotEmpty().WithMessage("Voucher code is required.")
                .MaximumLength(50).WithMessage("Voucher code must not exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Voucher code can only contain alphanumeric characters, hyphens, and underscores.");

            RuleFor(v => v.DiscountType)
                .IsInEnum().WithMessage("Invalid discount type.");

            RuleFor(v => v.DiscountValue)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

            When(v => v.DiscountType == DiscountType.Percentage, () =>
            {
                RuleFor(v => v.DiscountValue)
                    .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100%.");
            });

            RuleFor(v => v.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(v => v.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(v => v.StartDate).WithMessage("End date must be greater than start date.");

            When(v => v.MaxUsage.HasValue, () =>
            {
                RuleFor(v => v.MaxUsage!.Value)
                    .GreaterThan(0).WithMessage("Max usage must be greater than 0 if specified.");
            });

            if (voucherRepository != null)
            {
                RuleFor(v => v)
                    .MustAsync(async (cmd, cancellation) =>
                    {
                        var normalizedCode = cmd.Code.Trim().ToUpper();
                        return !await voucherRepository.GetQueryable()
                            .AnyAsync(v => v.Code.ToUpper() == normalizedCode && v.Id != cmd.Id, cancellation);
                    })
                    .WithMessage(cmd => $"Voucher with code '{cmd.Code}' already exists.")
                    .When(cmd => !string.IsNullOrEmpty(cmd.Code));
            }
        }
    }
}
