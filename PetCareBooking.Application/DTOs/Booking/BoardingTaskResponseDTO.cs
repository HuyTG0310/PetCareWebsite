using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Booking
{
    public class BoardingTaskResponseDTO
    {
        public Guid BookingId { get; set; }
        public Guid ItemId { get; set; }

        // Customer Info
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string? CustomerEmail { get; set; }

        // Pet Info
        public Guid PetId { get; set; }
        public string PetName { get; set; } = null!;
        public PetSpecies PetSpecies { get; set; }
        public string? PetBreed { get; set; }
        public decimal PetWeight { get; set; }
        public string? PetHealthNotes { get; set; }
        public string? PetAvatarUrl { get; set; }

        // Service Info
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public ServiceType ServiceType { get; set; }

        // Room Info (Hotel Room)
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomTypeName { get; set; }

        // Assigned Staff Info
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }

        // Schedule & Stay Duration
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public int TotalStayDays => ScheduledEndAt.HasValue && ScheduledEndAt > ScheduledStartAt
            ? (int)Math.Max(1, Math.Ceiling((ScheduledEndAt.Value.Date - ScheduledStartAt.Date).TotalDays))
            : 1;

        public int DaysRemaining
        {
            get
            {
                if (!ScheduledEndAt.HasValue) return 0;
                var remaining = (int)Math.Ceiling((ScheduledEndAt.Value.Date - DateTime.UtcNow.Date).TotalDays);
                return remaining < 0 ? 0 : remaining;
            }
        }

        public decimal Quantity { get; set; }
        public BookingItemStatus Status { get; set; }
        public string? ResultNote { get; set; }

        // Care Record Tracking summary
        public int TotalCareRecords { get; set; }
        public DateTime? LatestCareRecordDate { get; set; }
        public string? LatestHealthStatus { get; set; }
        public string? LatestNote { get; set; }
        public string? LatestImageUrl { get; set; }
    }
}

