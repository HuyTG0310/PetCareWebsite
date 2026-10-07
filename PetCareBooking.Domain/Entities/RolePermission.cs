namespace PetCareBooking.Domain.Entities
{
    public class RolePermission
    {
        // Join table (Many-to-Many) does not require a separate Id
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
