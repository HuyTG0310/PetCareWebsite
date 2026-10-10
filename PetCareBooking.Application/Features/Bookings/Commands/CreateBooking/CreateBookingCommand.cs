using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }
        public List<BookingItemRequestDTO> BookingItems { get; set; } = new List<BookingItemRequestDTO>();
    }
}