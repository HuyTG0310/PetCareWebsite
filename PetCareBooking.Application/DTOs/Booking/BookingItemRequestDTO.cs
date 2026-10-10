namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingItemRequestDTO
    {
        public Guid PetId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public decimal Quantity { get; set; } = 1;
    }
}