using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingPayments
{
    public class GetBookingPaymentsQuery : IRequest<ApiResponse<List<PaymentTransactionResponseDTO>>>
    {
        public Guid BookingId { get; set; }

        public GetBookingPaymentsQuery(Guid bookingId)
        {
            BookingId = bookingId;
        }
    }
}

