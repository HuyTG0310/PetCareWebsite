using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.UpdateRoomType
{
    public class UpdateRoomTypeCommand : IRequest<ApiResponse<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
