using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings
{
    public class GetAdminBookingsQuery : IRequest<ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        public BookingStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}