using PetCareBooking.Domain.Common;

namespace PetCareBooking.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid BookingItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual BookingItem BookingItem { get; set; } = null!;
    }
}
