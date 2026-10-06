using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommand : IRequest<ApiResponse<Guid>>
    {
        public Guid RoomTypeId { get; set; }
        public string RoomName { get; set; } = null!;
        public RoomStatus? Status { get; set; } = RoomStatus.Available;
    }
}
