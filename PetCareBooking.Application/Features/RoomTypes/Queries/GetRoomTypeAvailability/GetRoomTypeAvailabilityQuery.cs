using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeAvailability
{
    public class GetRoomTypeAvailabilityQuery : IRequest<ApiResponse<List<RoomTypeAvailabilityResponseDTO>>>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
