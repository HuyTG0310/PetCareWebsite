    using PetCareBooking.Domain.Enums;
    
    public class RoomSummaryDTO
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = null!;
        public RoomStatus Status { get; set; }
    }