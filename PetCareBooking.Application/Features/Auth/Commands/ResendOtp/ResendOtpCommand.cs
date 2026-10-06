using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Auth.Commands.ResendOtp
{
    public class ResendOtpCommand : IRequest<ApiResponse<bool>>
    {
        public string Email { get; set; } = null!;
    }
}