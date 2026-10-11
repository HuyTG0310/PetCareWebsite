using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetCheckoutSummary
{
    public class GetCheckoutSummaryQuery : IRequest<ApiResponse<CheckoutSummaryResponseDTO>>
    {
        public Guid BookingId { get; set; }

        public GetCheckoutSummaryQuery(Guid bookingId)
        {
            BookingId = bookingId;
        }
    }
}

