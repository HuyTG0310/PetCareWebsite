using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetRooms
{
    public class GetRoomsQuery : IRequest<ApiResponse<PagedResult<RoomResponseDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? RoomTypeId { get; set; }
        public RoomStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
    }
}
