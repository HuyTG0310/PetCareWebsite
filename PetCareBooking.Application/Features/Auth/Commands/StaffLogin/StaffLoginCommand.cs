using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Auth;

namespace PetCareBooking.Application.Features.Auth.Commands.StaffLogin
{
    public class StaffLoginCommand : IRequest<ApiResponse<StaffLoginResponseDto>>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
