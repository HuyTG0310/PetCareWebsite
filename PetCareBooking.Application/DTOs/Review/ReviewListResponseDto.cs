namespace PetCareBooking.Application.DTOs.Review
{
    public class ReviewListResponseDto
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerAvatarUrl { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
