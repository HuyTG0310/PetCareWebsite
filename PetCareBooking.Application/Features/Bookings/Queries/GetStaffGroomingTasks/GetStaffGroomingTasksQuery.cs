using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetStaffGroomingTasks
{
    public class GetStaffGroomingTasksQuery : IRequest<ApiResponse<PagedResult<GroomingTaskResponseDTO>>>
    {
        public Guid StaffId { get; set; }
        public BookingItemStatus? Status { get; set; }
        public DateTime? Date { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
