namespace PetCareBooking.Application.DTOs.Role
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string PermissionCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
        public string ModuleName { get; set; } = null!;
    }
}