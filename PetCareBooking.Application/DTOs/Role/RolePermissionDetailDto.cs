namespace PetCareBooking.Application.DTOs.Role
{
    public class RolePermissionDetailDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public List<PermissionDto> AssignedPermissions { get; set; } = new();
        public List<PermissionDto> AllAvailablePermissions { get; set; } = new();
    }
}