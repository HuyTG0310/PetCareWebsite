using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.CreateRoomType
{
    public class CreateRoomTypeCommand : IRequest<ApiResponse<Guid>>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
