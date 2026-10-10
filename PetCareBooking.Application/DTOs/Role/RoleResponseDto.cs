namespace PetCareBooking.Application.DTOs.Role
{
    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalPermissions { get; set; }
    }
}