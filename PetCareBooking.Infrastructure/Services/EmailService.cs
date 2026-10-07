using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using PetCareBooking.Application.Interfaces;

namespace PetCareBooking.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderName = _configuration["EmailSettings:SenderName"] ?? "PetCare Booking";
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            var username = _configuration["EmailSettings:Username"] ?? senderEmail;
            var password = _configuration["EmailSettings:Password"] ?? "";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "OTP Verification Code - PetCare Booking";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style=""font-family: Arial, sans-serif; padding: 20px; color: #333; max-width: 500px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px;"">
                        <h2 style=""color: #4CAF50; text-align: center;"">PetCare Booking</h2>
                        <p>Hello,</p>
                        <p>Your OTP verification code is:</p>
                        <div style=""background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #333; border-radius: 6px; margin: 20px 0;"">
                            {otpCode}
                        </div>
                        <p>This OTP code is valid for <b>5 minutes</b>. Please do not share this code with anyone.</p>
                        <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"" />
                        <p style=""font-size: 12px; color: #888; text-align: center;"">Automated email from PetCare Booking system, please do not reply.</p>
                    </div>"
            };

            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, cancellationToken);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("OTP email sent successfully to: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email to: {Email}", toEmail);
                throw new Exception("Unable to send verification email. Please check Email/Password configuration or try again later.", ex);
            }
        }
    }
}