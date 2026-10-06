using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Pet
{
    public class PetResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? OwnerName { get; set; }
        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public int? Age { get; set; }
        public string? HealthNotes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
