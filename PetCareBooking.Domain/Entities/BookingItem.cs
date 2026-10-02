using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class BookingItem : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid PetId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? RoomId { get; set; }
        public Guid? StaffId { get; set; }

        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AssignedPrice { get; set; }

        public BookingItemStatus Status { get; set; }
        public string? ResultNote { get; set; }
        public string? ResultImageUrl { get; set; }

        // Navigation Properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual Pet Pet { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual Room? Room { get; set; }
        public virtual Staff? Staff { get; set; }

        public virtual Review? Review { get; set; }
        public virtual ICollection<CareRecord> CareRecords { get; set; } = new List<CareRecord>();
    }
}
