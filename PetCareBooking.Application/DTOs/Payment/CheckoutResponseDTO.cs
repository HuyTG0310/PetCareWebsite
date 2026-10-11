using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Payment
{
    public class CheckoutResponseDTO
    {
        public Guid BookingId { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public decimal AmountPaid { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime PaidAt { get; set; }
        public List<string> ReleasedRooms { get; set; } = new List<string>();
        public string Message { get; set; } = null!;
    }
}

