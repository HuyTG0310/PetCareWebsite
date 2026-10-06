using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommand : IRequest<ApiResponse<bool>>
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}