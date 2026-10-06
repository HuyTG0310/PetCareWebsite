namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingItemRequestDTO
    {
        public Guid PetId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public decimal Quantity { get; set; } = 1;
        public Guid? RoomId { get; set; }
        public Guid? RoomTypeId { get; set; }
        public Guid? StaffId { get; set; }
    }
}