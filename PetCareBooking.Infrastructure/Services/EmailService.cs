using Microsoft.Extensions.Logging;
using PetCareBooking.Application.Interfaces;

namespace PetCareBooking.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default)
        {
            // In mã OTP ra cửa sổ Output / Console để lấy test trên Swagger
            _logger.LogInformation("==================================================");
            _logger.LogInformation("[EMAIL OTP] Gửi tới: {Email}", toEmail);
            _logger.LogInformation("[EMAIL OTP] Mã xác thực của bạn: {OtpCode}", otpCode);
            _logger.LogInformation("==================================================");

            return Task.CompletedTask;
        }
    }
}