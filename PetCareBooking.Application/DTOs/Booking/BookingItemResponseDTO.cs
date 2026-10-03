using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class BookingItemResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PetId { get; set; }
        public string PetName { get; set; } = null!;
        public PetSpecies PetSpecies { get; set; }
        public decimal PetWeight { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public ServiceType ServiceType { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AssignedPrice { get; set; }
        public BookingItemStatus Status { get; set; }
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? ResultNote { get; set; }
        public string? ResultImageUrl { get; set; }
    }
}