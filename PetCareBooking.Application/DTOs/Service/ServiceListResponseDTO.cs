using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Service
{
    public class ServiceListResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool IsActive { get; set; }
        // NO prices in list view
    }
}