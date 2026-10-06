namespace PetCareBooking.Application.Features.Auth.Commands.Login
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}