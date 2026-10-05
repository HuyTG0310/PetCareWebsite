using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Customer
{
    public class PetSummaryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public string? HealthNotes { get; set; }
        public bool IsActive { get; set; }
    }
}