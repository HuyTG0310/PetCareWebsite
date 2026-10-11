using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Staffs.Commands.CreateStaff
{
    public class CreateStaffCommand : IRequest<ApiResponse<Guid>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int YearsOfExperience { get; set; }
        public List<Guid> RoleIds { get; set; } = new();
    }
}