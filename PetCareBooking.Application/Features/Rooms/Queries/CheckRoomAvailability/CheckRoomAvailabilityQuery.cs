using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;

namespace PetCareBooking.Application.Features.Rooms.Queries.CheckRoomAvailability
{
    public class CheckRoomAvailabilityQuery : IRequest<ApiResponse<RoomAvailabilityCalendarResponseDTO>>
    {
        public Guid? ServiceId { get; set; }
        public Guid? RoomTypeId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

