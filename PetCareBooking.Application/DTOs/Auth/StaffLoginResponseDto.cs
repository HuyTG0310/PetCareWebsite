namespace PetCareBooking.Application.DTOs.Auth
{
    public class StaffLoginResponseDto
    {
        public string Token { get; set; } = null!;
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}