using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQuery : IRequest<ApiResponse<BookingResponseDTO>>
    {
        public Guid Id { get; set; }
    }
}