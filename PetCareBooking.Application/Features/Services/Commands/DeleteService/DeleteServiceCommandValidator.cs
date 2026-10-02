using FluentValidation;

namespace PetCareBooking.Application.Features.Services.Commands.DeleteService
{
    public class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
    {
        public DeleteServiceCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Service ID is required.");
        }
    }
}
