using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        // Inject ILogger để ghi lại lỗi chi tiết trên server
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Ghi log lỗi ngầm định để Developer có thể trace bug
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            // 2. Format Response mặc định (Lỗi 500)
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                // BẢO MẬT: Trả thông báo chung chung cho Client thay vì ném thẳng exception.Message
                Message = "An unexpected error occurred on the server. Please contact support.",
                Result = null
            };

            // 3. Phân loại lỗi và ghi đè Status Code / Message
            switch (exception)
            {
                case ValidationException validationException:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    var errorMessages = validationException.Errors.Select(e => e.ErrorMessage);
                    response.Message = string.Join(" | ", errorMessages);
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "You do not have permission to access this resource.";
                    break;

                    // Bạn có thể thêm case cho NotFoundException hoặc BadRequestException tự định nghĩa ở đây
            }

            // 4. Gửi Response
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = response.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true; // Báo cho .NET biết exception đã được xử lý xong
        }
    }
}