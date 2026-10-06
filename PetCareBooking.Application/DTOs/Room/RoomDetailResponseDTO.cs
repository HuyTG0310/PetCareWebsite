using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Room
{
    public class RoomDetailResponseDTO
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public string? RoomTypeDescription { get; set; }
        public RoomStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
