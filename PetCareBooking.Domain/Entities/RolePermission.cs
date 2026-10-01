namespace PetCareBooking.Domain.Entities
{
    public class RolePermission
    {
        // Bảng trung gian (Many-to-Many) không cần Id riêng
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
