using FluentValidation.TestHelper;
using PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord;
using PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord;
using Xunit;

namespace PetCareBooking.UnitTests.Features.CareRecords.Validators
{
    public class CareRecordCommandValidatorsTests
    {
        private readonly CreateCareRecordCommandValidator _createValidator = new();
        private readonly UpdateCareRecordCommandValidator _updateValidator = new();
        private readonly DeleteCareRecordCommandValidator _deleteValidator = new();

        [Fact]
        public void CreateValidator_ValidCommand_ShouldNotHaveErrors()
        {
            var command = new CreateCareRecordCommand
            {
                BookingItemId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                RecordDate = DateTime.UtcNow,
                HealthStatus = "Ổn định",
                Note = "Thú cưng ăn ngoan",
                ImageUrl = "https://example.com/image.jpg"
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreateValidator_EmptyBookingItemId_ShouldHaveError()
        {
            var command = new CreateCareRecordCommand
            {
                BookingItemId = Guid.Empty,
                StaffId = Guid.NewGuid(),
                RecordDate = DateTime.UtcNow
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.BookingItemId)
                  .WithErrorMessage("BookingItemId is required.");
        }

        [Fact]
        public void CreateValidator_EmptyStaffId_ShouldHaveError()
        {
            var command = new CreateCareRecordCommand
            {
                BookingItemId = Guid.NewGuid(),
                StaffId = Guid.Empty,
                RecordDate = DateTime.UtcNow
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.StaffId)
                  .WithErrorMessage("StaffId is required.");
        }

        [Fact]
        public void CreateValidator_HealthStatusExceeds100Chars_ShouldHaveError()
        {
            var command = new CreateCareRecordCommand
            {
                BookingItemId = Guid.NewGuid(),
                StaffId = Guid.NewGuid(),
                RecordDate = DateTime.UtcNow,
                HealthStatus = new string('A', 101)
            };

            var result = _createValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.HealthStatus)
                  .WithErrorMessage("Health status cannot exceed 100 characters.");
        }

        [Fact]
        public void UpdateValidator_EmptyId_ShouldHaveError()
        {
            var command = new UpdateCareRecordCommand
            {
                Id = Guid.Empty,
                HealthStatus = "Tốt"
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("CareRecord ID is required.");
        }

        [Fact]
        public void UpdateValidator_ValidCommand_ShouldNotHaveErrors()
        {
            var command = new UpdateCareRecordCommand
            {
                Id = Guid.NewGuid(),
                HealthStatus = "Tốt",
                Note = "Đã tắm sấy xong"
            };

            var result = _updateValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void DeleteValidator_EmptyId_ShouldHaveError()
        {
            var command = new DeleteCareRecordCommand(Guid.Empty);

            var result = _deleteValidator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("CareRecord ID is required.");
        }

        [Fact]
        public void DeleteValidator_ValidId_ShouldNotHaveErrors()
        {
            var command = new DeleteCareRecordCommand(Guid.NewGuid());

            var result = _deleteValidator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
