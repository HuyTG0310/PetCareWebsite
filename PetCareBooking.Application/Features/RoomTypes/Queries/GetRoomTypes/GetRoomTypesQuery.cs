using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypes
{
    public class GetRoomTypesQuery : IRequest<ApiResponse<List<RoomTypeResponseDTO>>>
    {
        public string? SearchTerm { get; set; }
    }
}
