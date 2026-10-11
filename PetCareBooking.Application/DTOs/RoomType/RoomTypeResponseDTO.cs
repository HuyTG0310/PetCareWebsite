namespace PetCareBooking.Application.DTOs.RoomType
{
    public class RoomTypeResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalRooms { get; set; }
    }
}
