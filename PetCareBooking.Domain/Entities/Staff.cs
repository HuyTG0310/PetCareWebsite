using PetCareBooking.Domain.Common;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Domain.Entities
{
    public class Staff : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public StaffStatus Status { get; set; }

        public virtual ICollection<StaffRole> StaffRoles { get; set; } = new List<StaffRole>();
    }
}
