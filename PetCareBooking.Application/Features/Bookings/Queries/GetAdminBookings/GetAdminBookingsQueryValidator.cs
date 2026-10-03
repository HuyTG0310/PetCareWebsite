using FluentValidation;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings
{
    public class GetAdminBookingsQueryValidator : AbstractValidator<GetAdminBookingsQuery>
    {
        public GetAdminBookingsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid booking status.")
                .When(x => x.Status.HasValue);

            RuleFor(x => x.DateTo)
                .GreaterThanOrEqualTo(x => x.DateFrom)
                .WithMessage("Date 'To' must be greater than or equal to Date 'From'.")
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
        }
    }
}