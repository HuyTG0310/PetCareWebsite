using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using PetCareBooking.Application.Common.Models;

namespace PetCareBooking.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        // Inject ILogger to log detailed server errors
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. Log unhandled exception for developers to trace bugs
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            // 2. Default Response format (500 Internal Server Error)
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status500InternalServerError,
                // SECURITY: Return generic message to Client instead of raw exception.Message
                Message = "An unexpected error occurred on the server. Please contact support.",
                Result = null
            };

            // 3. Classify exception and override Status Code / Message
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

                    // You can add cases for custom NotFoundException or BadRequestException here
            }

            // 4. Send Response
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = response.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true; // Signal to .NET that the exception has been handled
        }
    }
}