using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
    {
        public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Đăng xuất thành công! Vui lòng xóa Token ở phía người dùng.",
                Result = true
            };
        }
    }
}