namespace PetCareBooking.Application.DTOs.Review
{
    public class ReviewDetailResponseDto
    {
        public Guid Id { get; set; }
        public Guid BookingItemId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}