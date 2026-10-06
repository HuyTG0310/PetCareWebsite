using FluentValidation;

namespace PetCareBooking.Application.Features.Pets.Commands.DeletePet
{
    public class DeletePetCommandValidator : AbstractValidator<DeletePetCommand>
    {
        public DeletePetCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Pet ID is required.");
        }
    }
}
