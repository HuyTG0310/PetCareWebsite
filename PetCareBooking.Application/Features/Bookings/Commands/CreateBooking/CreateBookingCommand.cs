using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<ApiResponse<CreateBookingResponseDTO>>
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public string PaymentChoice { get; set; } = "PAY_AT_COUNTER";

        public List<BookingItemRequestDTO> BookingItems { get; set; } = new List<BookingItemRequestDTO>();
    }
}