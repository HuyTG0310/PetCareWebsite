using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Staff;

namespace PetCareBooking.Application.Features.Staffs.Commands.UpdateStaffProfile
{
    public class UpdateStaffProfileCommand : IRequest<ApiResponse<StaffProfileResponseDto>>
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int YearsOfExperience { get; set; }
    }
}
