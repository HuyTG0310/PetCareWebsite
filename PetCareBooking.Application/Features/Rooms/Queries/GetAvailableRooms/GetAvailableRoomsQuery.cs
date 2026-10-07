using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetAvailableRooms
{
    public class GetAvailableRoomsQuery : IRequest<ApiResponse<List<AvailableRoomResponseDTO>>>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? RoomTypeId { get; set; }
    }
}
