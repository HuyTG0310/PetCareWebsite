namespace PetCareBooking.Application.DTOs.RoomType
{
    public class RoomTypeAvailabilityResponseDTO
    {
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public bool IsAvailable => AvailableRooms > 0;
    }
}
