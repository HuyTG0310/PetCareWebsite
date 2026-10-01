using Microsoft.AspNetCore.Diagnostics;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.API.Middlewares
{
    // bắt lỗi toàn dự án như db, validation
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = exception.Message,
                Result = null
            };

            // Bắt lỗi Validation từ FluentValidation
            if (exception is FluentValidation.ValidationException validationException)
            {
                var errorMessages = validationException.Errors.Select(e => e.ErrorMessage);
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = string.Join(" | ", errorMessages);
            }

            httpContext.Response.StatusCode = response.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
