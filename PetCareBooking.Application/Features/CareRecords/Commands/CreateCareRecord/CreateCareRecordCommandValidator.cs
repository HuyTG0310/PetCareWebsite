using FluentValidation;

namespace PetCareBooking.Application.Features.CareRecords.Commands.CreateCareRecord
{
    public class CreateCareRecordCommandValidator : AbstractValidator<CreateCareRecordCommand>
    {
        public CreateCareRecordCommandValidator()
        {
            RuleFor(x => x.BookingItemId)
                .NotEmpty().WithMessage("BookingItemId is required.");

            RuleFor(x => x.StaffId)
                .NotEmpty().WithMessage("StaffId is required.");

            RuleFor(x => x.RecordDate)
                .NotEmpty().WithMessage("RecordDate is required.");

            RuleFor(x => x.HealthStatus)
                .MaximumLength(100).WithMessage("Health status cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.HealthStatus));

            RuleFor(x => x.ImageUrl)
                .MaximumLength(2000).WithMessage("ImageUrl cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }
    }
}
