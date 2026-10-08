using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid? VoucherId { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string? CancellationReason { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public virtual Voucher? Voucher { get; set; }
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}
