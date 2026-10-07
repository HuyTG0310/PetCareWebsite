using FluentValidation;

namespace PetCareBooking.Application.Features.CareRecords.Commands.DeleteCareRecord
{
    public class DeleteCareRecordCommandValidator : AbstractValidator<DeleteCareRecordCommand>
    {
        public DeleteCareRecordCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("CareRecord ID is required.");
        }
    }
}
