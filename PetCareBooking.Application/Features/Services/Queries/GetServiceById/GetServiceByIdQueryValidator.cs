using FluentValidation;

namespace PetCareBooking.Application.Features.Services.Queries.GetServiceById
{
    public class GetServiceByIdQueryValidator : AbstractValidator<GetServiceByIdQuery>
    {
        public GetServiceByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Service ID is required.");
        }
    }
}
