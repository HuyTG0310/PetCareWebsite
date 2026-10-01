namespace PetCareBooking.Domain.Entities
{
    public class CareRecord
    {
        public Guid Id { get; set; }
        public Guid BookingItemId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime RecordDate { get; set; }
        public string? HealthStatus { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }

        public virtual BookingItem BookingItem { get; set; } = null!;
        public virtual Staff Staff { get; set; } = null!;
    }
}
