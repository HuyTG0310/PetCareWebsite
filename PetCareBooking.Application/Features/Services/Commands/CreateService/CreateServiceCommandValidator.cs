using FluentValidation;

namespace PetCareBooking.Application.Features.Services.Commands.CreateService
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Service name is required.")
                .MaximumLength(150).WithMessage("Service name cannot exceed 150 characters.");

            RuleFor(x => x.ServiceType)
                .IsInEnum().WithMessage("Invalid service type.");

            RuleForEach(x => x.Prices).ChildRules(prices =>
            {
                prices.RuleFor(p => p.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

                prices.RuleFor(p => p.PricingUnit)
                    .IsInEnum().WithMessage("Invalid pricing unit."); // Automatically validate valid Enum

                prices.RuleFor(p => p.MinWeight)
                    .GreaterThanOrEqualTo(0).WithMessage("Min weight must be greater than or equal to 0.")
                    .When(p => p.MinWeight.HasValue);

                prices.RuleFor(p => p.MaxWeight)
                    .GreaterThan(0).WithMessage("Max weight must be greater than 0.")
                    .When(p => p.MaxWeight.HasValue);

                prices.RuleFor(p => p.MaxWeight)
                    .Must((dto, maxWeight) => maxWeight > dto.MinWeight)
                    .WithMessage("Max weight must be strictly greater than Min weight.")
                    .When(p => p.MinWeight.HasValue && p.MaxWeight.HasValue);
            });
        }
    }
}
