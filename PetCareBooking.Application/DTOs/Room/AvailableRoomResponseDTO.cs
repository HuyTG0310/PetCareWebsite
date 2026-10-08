using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Room
{
    public class AvailableRoomResponseDTO
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public RoomStatus Status { get; set; }
    }
}
