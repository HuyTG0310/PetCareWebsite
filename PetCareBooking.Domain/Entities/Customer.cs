using PetCareBooking.Domain.Common;

namespace PetCareBooking.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
