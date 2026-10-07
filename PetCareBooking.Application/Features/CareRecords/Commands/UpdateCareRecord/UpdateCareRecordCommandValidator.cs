using FluentValidation;

namespace PetCareBooking.Application.Features.CareRecords.Commands.UpdateCareRecord
{
    public class UpdateCareRecordCommandValidator : AbstractValidator<UpdateCareRecordCommand>
    {
        public UpdateCareRecordCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("CareRecord ID is required.");

            RuleFor(x => x.HealthStatus)
                .MaximumLength(100).WithMessage("Health status cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.HealthStatus));

            RuleFor(x => x.ImageUrl)
                .MaximumLength(2000).WithMessage("ImageUrl cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }
    }
}
