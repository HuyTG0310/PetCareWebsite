using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings
{
    public class GetCustomerBookingsQueryValidator : AbstractValidator<GetCustomerBookingsQuery>
    {
        public GetCustomerBookingsQueryValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid booking status.")
                .When(x => x.Status.HasValue);
        }
    }
}