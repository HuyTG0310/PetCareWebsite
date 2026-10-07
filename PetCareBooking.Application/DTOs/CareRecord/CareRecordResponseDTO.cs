namespace PetCareBooking.Application.DTOs.CareRecord
{
    public class CareRecordResponseDTO
    {
        public Guid Id { get; set; }
        public Guid BookingItemId { get; set; }
        public Guid StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? PetName { get; set; }
        public DateTime RecordDate { get; set; }
        public string? HealthStatus { get; set; }
        public string? Note { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
