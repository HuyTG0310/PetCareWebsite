using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoomStatus
{
    public class UpdateRoomStatusCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public RoomStatus Status { get; set; }
    }
}
