using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingReceipt
{
    public class GetBookingReceiptQuery : IRequest<ApiResponse<BookingReceiptResponseDTO>>
    {
        public Guid BookingId { get; set; }

        public GetBookingReceiptQuery(Guid bookingId)
        {
            BookingId = bookingId;
        }
    }
}

