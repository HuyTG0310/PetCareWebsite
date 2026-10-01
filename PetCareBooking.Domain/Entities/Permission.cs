namespace PetCareBooking.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string PermissionCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
        public string ModuleName { get; set; } = null!;

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
