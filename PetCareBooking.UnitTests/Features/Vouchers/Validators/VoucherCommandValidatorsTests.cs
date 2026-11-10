using FluentValidation.TestHelper;
using PetCareBooking.Application.Features.Vouchers.Commands.CreateVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.DeleteVoucher;
using PetCareBooking.Application.Features.Vouchers.Commands.UpdateVoucher;
using PetCareBooking.Domain.Enums;
using Xunit;

namespace PetCareBooking.UnitTests.Features.Vouchers.Validators
{
    public class VoucherCommandValidatorsTests
    {
        private readonly CreateVoucherCommandValidator _createValidator = new();
        private readonly UpdateVoucherCommandValidator _updateValidator = new();
        private readonly DeleteVoucherCommandValidator _deleteValidator = new();

        [Fact]
        public void CreateValidator_ValidCommand_ShouldNotHaveErrors()
        {
            var command = new CreateVoucherCommand
            {
                Code = "SUMMER2026",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                MaxUsage = 100
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreateValidator_EmptyCode_ShouldHaveError()
        {
            var command = new CreateVoucherCommand
            {
                Code = "",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 50000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Code)
                  .WithErrorMessage("Voucher code is required.");
        }

        [Fact]
        public void CreateValidator_InvalidCodeCharacters_ShouldHaveError()
        {
            var command = new CreateVoucherCommand
            {
                Code = "CODE WITH SPACES!@#",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 50000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public void CreateValidator_DiscountValueZeroOrNegative_ShouldHaveError()
        {
            var command = new CreateVoucherCommand
            {
                Code = "DISCOUNT0",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.DiscountValue)
                  .WithErrorMessage("Discount value must be greater than 0.");
        }

        [Fact]
        public void CreateValidator_PercentageExceeds100_ShouldHaveError()
        {
            var command = new CreateVoucherCommand
            {
                Code = "SUPER150",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 150,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.DiscountValue)
                  .WithErrorMessage("Percentage discount cannot exceed 100%.");
        }

        [Fact]
        public void CreateValidator_EndDateBeforeStartDate_ShouldHaveError()
        {
            var now = DateTime.UtcNow;
            var command = new CreateVoucherCommand
            {
                Code = "EXPIRED",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10,
                StartDate = now,
                EndDate = now.AddDays(-1)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.EndDate)
                  .WithErrorMessage("End date must be greater than start date.");
        }

        [Fact]
        public void UpdateValidator_EmptyId_ShouldHaveError()
        {
            var command = new UpdateVoucherCommand
            {
                Id = Guid.Empty,
                Code = "UPDATE1",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 20000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Voucher ID is required.");
        }

        [Fact]
        public void UpdateValidator_ValidCommand_ShouldNotHaveErrors()
        {
            var command = new UpdateVoucherCommand
            {
                Id = Guid.NewGuid(),
                Code = "VALIDUPDATE",
                DiscountType = DiscountType.Fixed_Amount,
                DiscountValue = 30000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void DeleteValidator_EmptyId_ShouldHaveError()
        {
            var command = new DeleteVoucherCommand(Guid.Empty);

            var result = _deleteValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Voucher ID is required.");
        }

        [Fact]
        public void DeleteValidator_ValidId_ShouldNotHaveErrors()
        {
            var command = new DeleteVoucherCommand(Guid.NewGuid());

            var result = _deleteValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
