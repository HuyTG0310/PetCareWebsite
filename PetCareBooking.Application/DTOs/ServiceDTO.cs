namespace PetCareBooking.Application.DTOs
{
    public class ServiceDTO
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = null!;
        public string ServiceType { get; set; } = null!;
        public decimal Price { get; set; }
        public string Status { get; set; } = null!;
    }
}
