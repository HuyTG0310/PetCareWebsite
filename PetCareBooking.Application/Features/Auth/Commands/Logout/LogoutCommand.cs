using MediatR;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<ApiResponse<bool>>
    {
    }
}