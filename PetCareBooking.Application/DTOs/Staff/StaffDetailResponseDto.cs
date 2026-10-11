using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.DTOs.Staff
{
    public class StaffDetailResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int YearsOfExperience { get; set; }
        public StaffStatus Status { get; set; }
        public List<Guid> RoleIds { get; set; } = new();
        public List<string> RoleNames { get; set; } = new();
    }
}