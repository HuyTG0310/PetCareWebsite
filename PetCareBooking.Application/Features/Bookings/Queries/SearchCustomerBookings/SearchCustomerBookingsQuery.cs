using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.SearchCustomerBookings
{
    public class SearchCustomerBookingsQuery : IRequest<ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        public Guid CustomerId { get; set; }
        public string? Keyword { get; set; }  // Search in booking items only
        public BookingStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}