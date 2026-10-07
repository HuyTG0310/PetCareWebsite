namespace PetCareBooking.Application.DTOs.CareRecord
{
    public class CareRecordListResponseDTO
    {
        public Guid Id { get; set; }
        public Guid BookingItemId { get; set; }
        public Guid StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime RecordDate { get; set; }
        public string? HealthStatus { get; set; }
        public string? ImageUrl { get; set; }
    }
}
