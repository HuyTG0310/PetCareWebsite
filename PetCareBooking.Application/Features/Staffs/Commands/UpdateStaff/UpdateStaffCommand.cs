using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Domain.Enums;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int YearsOfExperience { get; set; }
        public StaffStatus Status { get; set; }
        public List<Guid> RoleIds { get; set; } = new();
    }
}