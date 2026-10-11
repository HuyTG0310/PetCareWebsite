namespace PetCareBooking.Application.DTOs.Payment
{
    public class VnPayCallbackResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = null!;
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionNo { get; set; }
        public string? BankCode { get; set; }
        public string? ResponseCode { get; set; }
        public DateTime? PayDate { get; set; }
    }
}

