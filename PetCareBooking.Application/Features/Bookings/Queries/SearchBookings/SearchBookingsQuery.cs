using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.SearchBookings
{
    /// <summary>
    /// ADMIN ONLY - Search all bookings by customer name, email, phone
    /// </summary>
    public class SearchBookingsQuery : IRequest<ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        public string? Keyword { get; set; }  // Search by customer name/email/phone
        public BookingStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}