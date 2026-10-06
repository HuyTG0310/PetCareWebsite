using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Pet
{
    public class PetListResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public bool IsActive { get; set; }
    }
}
