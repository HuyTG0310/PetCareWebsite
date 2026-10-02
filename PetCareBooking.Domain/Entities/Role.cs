using PetCareBooking.Domain.Common;

namespace PetCareBooking.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<StaffRole> StaffRoles { get; set; } = new List<StaffRole>();
    }
}
