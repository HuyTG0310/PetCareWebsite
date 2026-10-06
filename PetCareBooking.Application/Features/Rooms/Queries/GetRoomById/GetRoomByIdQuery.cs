using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQuery : IRequest<ApiResponse<RoomDetailResponseDTO>>
    {
        public Guid Id { get; set; }
    }
}
