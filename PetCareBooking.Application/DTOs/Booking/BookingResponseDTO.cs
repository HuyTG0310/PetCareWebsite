using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public Guid? PromotionId { get; set; }
        public string? PromotionCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BookingItemResponseDTO> BookingItems { get; set; } = new List<BookingItemResponseDTO>();
    }
}