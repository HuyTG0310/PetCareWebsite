namespace PetCareBooking.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid BookingItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual BookingItem BookingItem { get; set; } = null!;
    }
}
