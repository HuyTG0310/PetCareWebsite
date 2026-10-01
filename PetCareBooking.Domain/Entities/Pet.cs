using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Pet
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public decimal Weight { get; set; }
        public int? Age { get; set; }
        public string? HealthNotes { get; set; }
        public bool IsActive { get; set; }
        public virtual Customer Owner { get; set; } = null!;
    }
}
