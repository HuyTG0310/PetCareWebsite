using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeById
{
    public class GetRoomTypeByIdQuery : IRequest<ApiResponse<RoomTypeDetailResponseDTO>>
    {
        public Guid Id { get; set; }
    }
}
