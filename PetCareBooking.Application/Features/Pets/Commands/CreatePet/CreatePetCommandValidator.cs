using FluentValidation;

namespace PetCareBooking.Application.Features.Pets.Commands.CreatePet
{
    public class CreatePetCommandValidator : AbstractValidator<CreatePetCommand>
    {
        public CreatePetCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Pet name is required.")
                .MaximumLength(100).WithMessage("Pet name cannot exceed 100 characters.");

            RuleFor(x => x.Species)
                .IsInEnum().WithMessage("Invalid pet species.");

            RuleFor(x => x.Breed)
                .MaximumLength(100).WithMessage("Breed cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Breed));

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0).WithMessage("Age must be greater than or equal to 0.")
                .When(x => x.Age.HasValue);

            RuleFor(x => x.HealthNotes)
                .MaximumLength(1000).WithMessage("Health notes cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.HealthNotes));
        }
    }
}
