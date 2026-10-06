namespace PetCareBooking.Application.DTOs.RoomType
{
    public class RoomTypeDetailResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalRooms { get; set; }
        public List<RoomSummaryDTO> Rooms { get; set; } = new List<RoomSummaryDTO>();
    }
}
