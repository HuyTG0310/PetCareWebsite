using PetCareBooking.Domain.Common;

namespace PetCareBooking.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid BookingId { get; set; }
        public string TransactionType { get; set; } = null!; // DEPOSIT, PAYMENT
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!; // CASH, BANK_TRANSFER, CARD, E_WALLET
        public string PaymentStatus { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED, CANCELLED
        public string? TransactionCode { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Note { get; set; }

        public virtual Booking Booking { get; set; } = null!;
    }
}
