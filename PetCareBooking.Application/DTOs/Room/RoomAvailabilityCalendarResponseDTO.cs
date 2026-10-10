namespace PetCareBooking.Application.DTOs.Room
{
    public class DailyRoomAvailabilityDTO
    {
        public DateTime Date { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public bool IsAvailable => AvailableRooms > 0;
    }

    public class RoomAvailabilityCalendarResponseDTO
    {
        public Guid? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? IsFullRangeAvailable { get; set; }
        public int? MinAvailableRoomsInRange { get; set; }
        public List<DailyRoomAvailabilityDTO> DailyAvailabilities { get; set; } = new();
    }
}

