namespace PetCareBooking.Application.DTOs.Payment
{
    public class PaymentTransactionResponseDTO
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string TransactionType { get; set; } = null!; // DEPOSIT, PAYMENT
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!; // CASH, BANK_TRANSFER, CARD, E_WALLET, VNPAY
        public string PaymentStatus { get; set; } = null!; // PENDING, SUCCESS, FAILED, CANCELLED
        public string? TransactionCode { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

