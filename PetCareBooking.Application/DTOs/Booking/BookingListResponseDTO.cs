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

        public string BookingType { get; set; } = "Grooming"; // "Boarding" or "Grooming"
        public DateTime? ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public List<string> ServiceNames { get; set; } = new List<string>();
        public bool CanCheckIn { get; set; }
        public bool CanCheckout { get; set; }
        public bool HasInServiceItems { get; set; }
        public bool IsFullyAssigned { get; set; }
        public string? CancellationReason { get; set; }
        public bool CanMarkNoShow { get; set; }
        public bool IsNoShow { get; set; }
        public bool HasPhotos { get; set; }
    }
}