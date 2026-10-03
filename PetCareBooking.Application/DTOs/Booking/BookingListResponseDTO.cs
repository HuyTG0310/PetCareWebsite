using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingListResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}