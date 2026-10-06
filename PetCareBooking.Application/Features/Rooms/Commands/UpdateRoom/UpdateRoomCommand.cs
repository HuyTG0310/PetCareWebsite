using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public Guid RoomTypeId { get; set; }
        public string RoomName { get; set; } = null!;
    }
}
