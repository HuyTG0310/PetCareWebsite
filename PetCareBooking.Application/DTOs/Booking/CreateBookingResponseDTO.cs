namespace PetCareBooking.Application.DTOs.Booking
{
    public class CreateBookingResponseDTO
    {
        public Guid BookingId { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentChoice { get; set; } = "PAY_AT_COUNTER"; // PAY_AT_COUNTER, VNPAY
        public string? PaymentUrl { get; set; }
    }
}

