using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class GroomingTaskResponseDTO
    {
        public Guid BookingId { get; set; }
        public Guid ItemId { get; set; }

        // Customer Info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        // Pet Info
        public Guid PetId { get; set; }
        public string PetName { get; set; } = null!;
        public PetSpecies PetSpecies { get; set; }
        public string? PetBreed { get; set; }
        public decimal PetWeight { get; set; }
        public string? PetHealthNotes { get; set; }

        // Service Info
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public ServiceType ServiceType { get; set; }

        // Room/Table Info (Grooming table / Spa room)
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }

        // Schedule & Progress
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public int DurationMinutes => ScheduledEndAt.HasValue && ScheduledEndAt > ScheduledStartAt
            ? (int)(ScheduledEndAt.Value - ScheduledStartAt).TotalMinutes
            : 0;

        public decimal Quantity { get; set; }
        public BookingItemStatus Status { get; set; }

        // Result Info
        public string? ResultNote { get; set; }
        public string? ResultImageUrl { get; set; }
    }
}
