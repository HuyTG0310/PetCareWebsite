using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetStaffBoardingTasks
{
    public class GetStaffBoardingTasksQuery : IRequest<ApiResponse<PagedResult<BoardingTaskResponseDTO>>>
    {
        public Guid? StaffId { get; set; }
        public BookingItemStatus? Status { get; set; }
        public DateTime? Date { get; set; }
        public bool OnlyActiveGuests { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

