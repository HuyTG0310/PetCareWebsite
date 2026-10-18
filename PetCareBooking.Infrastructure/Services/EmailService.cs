using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
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
                client.Timeout = 10000;
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

        public async Task SendBookingConfirmationEmailAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            if (booking == null)
            {
                _logger.LogWarning("SendBookingConfirmationEmailAsync called with null booking.");
                return;
            }

            var customer = booking.Customer;
            if (customer == null || string.IsNullOrWhiteSpace(customer.Email))
            {
                _logger.LogWarning("Cannot send booking confirmation email: Customer or Customer.Email is missing for Booking ID: {BookingId}", booking.Id);
                return;
            }

            var toEmail = customer.Email.Trim();
            var customerName = !string.IsNullOrWhiteSpace(customer.FullName) ? customer.FullName : "Quý khách";
            var bookingCode = booking.Id.ToString()[..8].ToUpper();

            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderName = _configuration["EmailSettings:SenderName"] ?? "PetCare Booking";
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            var username = _configuration["EmailSettings:Username"] ?? senderEmail;
            var password = _configuration["EmailSettings:Password"] ?? "";

            // Determine payment method and status badge
            var successfulTxn = booking.PaymentTransactions?.FirstOrDefault(t => t.PaymentStatus == "SUCCESS");
            string paymentStatusBadge;
            if (successfulTxn != null)
            {
                paymentStatusBadge = $"<span style=\"background-color: #dcfce7; color: #15803d; padding: 4px 10px; border-radius: 9999px; font-size: 13px; font-weight: 600;\">Đã thanh toán ({successfulTxn.PaymentMethod})</span>";
            }
            else
            {
                paymentStatusBadge = "<span style=\"background-color: #fef9c3; color: #a16207; padding: 4px 10px; border-radius: 9999px; font-size: 13px; font-weight: 600;\">Thanh toán tại quầy khi nhận dịch vụ</span>";
            }

            // Generate Booking Items Rows
            var itemsHtml = new System.Text.StringBuilder();
            if (booking.BookingItems != null && booking.BookingItems.Any())
            {
                foreach (var item in booking.BookingItems)
                {
                    var petName = item.Pet?.Name ?? "Thú cưng";
                    var petInfo = item.Pet != null 
                        ? $"<div style=\"font-size: 12px; color: #64748b;\">Giống: {item.Pet.Breed ?? item.Pet.Species.ToString()} • {item.Pet.Weight:0.#}kg</div>"
                        : "";

                    var serviceName = item.Service?.Name ?? "Dịch vụ chăm sóc";
                    var roomInfo = item.Room != null
                        ? $"<div style=\"font-size: 12px; color: #0284c7; font-weight: 500;\">🏢 Phòng/Chuồng: {item.Room.RoomName}</div>"
                        : "";

                    var timeStr = item.ScheduledStartAt.ToString("HH:mm dd/MM/yyyy");
                    if (item.ScheduledEndAt.HasValue && item.ScheduledEndAt.Value > item.ScheduledStartAt)
                    {
                        timeStr += $"<br/><span style=\"font-size: 11px; color: #94a3b8;\">đến {item.ScheduledEndAt.Value:HH:mm dd/MM/yyyy}</span>";
                    }

                    itemsHtml.Append($@"
                    <tr style=""border-bottom: 1px solid #f1f5f9;"">
                        <td style=""padding: 12px 10px; vertical-align: top;"">
                            <strong style=""color: #0f172a; font-size: 14px;"">{petName}</strong>
                            {petInfo}
                        </td>
                        <td style=""padding: 12px 10px; vertical-align: top;"">
                            <strong style=""color: #0d9488; font-size: 14px;"">{serviceName}</strong>
                            {roomInfo}
                        </td>
                        <td style=""padding: 12px 10px; vertical-align: top; color: #334155; font-size: 13px;"">
                            {timeStr}
                        </td>
                        <td style=""padding: 12px 10px; vertical-align: top; text-align: right; font-weight: 600; color: #0f172a; font-size: 14px;"">
                            {item.AssignedPrice:N0} đ
                        </td>
                    </tr>");
                }
            }
            else
            {
                itemsHtml.Append(@"
                <tr>
                    <td colspan=""4"" style=""padding: 16px; text-align: center; color: #64748b;"">Chưa có thông tin dịch vụ chi tiết.</td>
                </tr>");
            }

            // Voucher row if any
            var voucherRow = "";
            if (booking.Voucher != null)
            {
                var discountStr = booking.Voucher.DiscountType == PetCareBooking.Domain.Enums.DiscountType.Percentage
                    ? $"{booking.Voucher.DiscountValue}%"
                    : $"{booking.Voucher.DiscountValue:N0} đ";

                voucherRow = $@"
                <tr>
                    <td colspan=""3"" style=""padding: 8px 10px; text-align: right; color: #64748b; font-size: 13px;"">
                        Mã giảm giá (<strong>{booking.Voucher.Code}</strong>):
                    </td>
                    <td style=""padding: 8px 10px; text-align: right; color: #16a34a; font-weight: 600; font-size: 14px;"">
                        -{discountStr}
                    </td>
                </tr>";
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(customerName, toEmail));
            message.Subject = $"[PetCare Booking] Xác nhận lịch hẹn thành công - #{bookingCode}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #334155;"">
    <div style=""max-width: 640px; margin: 24px auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden;"">
        
        <!-- Header Banner -->
        <div style=""background: linear-gradient(135deg, #0d9488 0%, #059669 100%); padding: 32px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0 0 8px 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px;"">🐾 PetCare Booking</h1>
            <p style=""margin: 0; font-size: 16px; opacity: 0.95;"">Đơn đặt lịch của bạn đã được xác nhận thành công!</p>
        </div>

        <div style=""padding: 24px 28px;"">
            <!-- Greeting -->
            <p style=""font-size: 15px; margin-top: 0; line-height: 1.6;"">
                Xin chào <strong>{customerName}</strong>,
            </p>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569;"">
                Cảm ơn bạn đã lựa chọn <strong>PetCare Booking</strong> để chăm sóc các bé cưng. Chúng tôi rất vui mừng thông báo rằng lịch hẹn của bạn đã được <strong style=""color: #059669;"">XÁC NHẬN CHÍNH THỨC</strong>.
            </p>

            <!-- Booking Info Card -->
            <div style=""background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 16px 20px; margin: 20px 0;"">
                <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
                    <tr>
                        <td style=""padding: 6px 0; color: #64748b;"">Mã đơn đặt lịch:</td>
                        <td style=""padding: 6px 0; font-weight: 700; color: #0f172a; text-align: right;"">#{bookingCode}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 6px 0; color: #64748b;"">Trạng thái:</td>
                        <td style=""padding: 6px 0; text-align: right;"">
                            <span style=""background-color: #dbeafe; color: #1d4ed8; padding: 4px 10px; border-radius: 9999px; font-size: 12px; font-weight: 600;"">ĐÃ XÁC NHẬN (CONFIRMED)</span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 6px 0; color: #64748b;"">Thanh toán:</td>
                        <td style=""padding: 6px 0; text-align: right;"">
                            {paymentStatusBadge}
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 6px 0; color: #64748b;"">Số điện thoại liên hệ:</td>
                        <td style=""padding: 6px 0; font-weight: 600; color: #0f172a; text-align: right;"">{customer.PhoneNumber ?? "Chưa cập nhật"}</td>
                    </tr>
                </table>
            </div>

            <!-- Services Detail Table -->
            <h3 style=""font-size: 16px; font-weight: 700; color: #0f172a; margin: 24px 0 12px 0;"">Chi tiết dịch vụ đã đặt</h3>
            <table style=""width: 100%; border-collapse: collapse; margin-bottom: 16px;"">
                <thead>
                    <tr style=""background-color: #f1f5f9; border-bottom: 2px solid #e2e8f0;"">
                        <th style=""padding: 10px; text-align: left; font-size: 13px; color: #475569; font-weight: 600;"">Thú cưng</th>
                        <th style=""padding: 10px; text-align: left; font-size: 13px; color: #475569; font-weight: 600;"">Dịch vụ</th>
                        <th style=""padding: 10px; text-align: left; font-size: 13px; color: #475569; font-weight: 600;"">Thời gian</th>
                        <th style=""padding: 10px; text-align: right; font-size: 13px; color: #475569; font-weight: 600;"">Giá</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                    {voucherRow}
                    <tr style=""border-top: 2px solid #cbd5e1;"">
                        <td colspan=""3"" style=""padding: 14px 10px; text-align: right; font-weight: 700; color: #0f172a; font-size: 15px;"">Tổng tiền thanh toán:</td>
                        <td style=""padding: 14px 10px; text-align: right; font-weight: 700; color: #e11d48; font-size: 18px;"">{booking.TotalPrice:N0} đ</td>
                    </tr>
                </tbody>
            </table>

            <!-- Salon Notes & Instructions -->
            <div style=""background-color: #ecfdf5; border-left: 4px solid #10b981; padding: 14px 16px; border-radius: 6px; margin: 24px 0;"">
                <h4 style=""margin: 0 0 8px 0; color: #065f46; font-size: 14px; font-weight: 700;"">📌 Hướng dẫn & Lưu ý khi đưa bé đến Salon:</h4>
                <ul style=""margin: 0; padding-left: 18px; font-size: 13px; color: #047857; line-height: 1.6;"">
                    <li>Quý khách vui lòng đưa bé đến trước giờ hẹn <strong>10 - 15 phút</strong> để các bạn kỹ thuật viên chuẩn bị chu đáo nhất.</li>
                    <li>Vui lòng mang theo <strong>sổ tiêm chủng</strong> hoặc thông báo nếu bé có tiền sử dị ứng, hen suyễn hay nhát người.</li>
                    <li>Nếu cần đổi giờ hẹn hoặc bận đột xuất, xin vui lòng liên hệ hotline hoặc thao tác trên website trước ít nhất <strong>2 giờ</strong> để được hỗ trợ.</li>
                </ul>
            </div>

            <!-- Contact & Salon Info -->
            <p style=""font-size: 14px; line-height: 1.6; color: #475569; margin-bottom: 8px;"">
                Nếu bạn cần trợ giúp khẩn cấp hoặc có bất kỳ câu hỏi nào, xin đừng ngần ngại liên hệ:
            </p>
            <p style=""font-size: 13px; line-height: 1.5; color: #64748b; margin: 4px 0;"">
                📞 Hotline: <strong>0901 234 567</strong> &nbsp;|&nbsp; 📧 Email: <strong>support@petcarebooking.com</strong><br/>
                🏠 Địa chỉ Salon: <strong>123 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh</strong>
            </p>

            <hr style=""border: none; border-top: 1px solid #e2e8f0; margin: 24px 0 16px 0;"" />

            <p style=""font-size: 12px; color: #94a3b8; text-align: center; margin: 0;"">
                Đây là email tự động gửi từ hệ thống PetCare Booking, vui lòng không trả lời trực tiếp email này.<br/>
                &copy; {DateTime.UtcNow.Year} PetCare Booking. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>"
            };

            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                client.Timeout = 10000;
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls, cancellationToken);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Booking confirmation email sent successfully to: {Email} for Booking ID: {BookingId}", toEmail, booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email to: {Email} for Booking ID: {BookingId}", toEmail, booking.Id);
                // We do not rethrow so email failures do not fail the booking workflow
            }
        }

        public void QueueBookingConfirmationEmail(Guid bookingId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Small delay to ensure the database commit of the calling request has finished
                    await Task.Delay(300);

                    using var scope = _scopeFactory.CreateScope();
                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Booking>>();

                    var bookings = await bookingRepo.FindAsync(
                        b => b.Id == bookingId,
                        "BookingItems.Pet", "BookingItems.Service", "BookingItems.Room", "Customer", "PaymentTransactions", "Voucher"
                    );

                    var booking = bookings.FirstOrDefault();
                    if (booking != null)
                    {
                        await SendBookingConfirmationEmailAsync(booking, CancellationToken.None);
                    }
                    else
                    {
                        _logger.LogWarning("Background email confirmation skipped: Booking {BookingId} not found.", bookingId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background email confirmation failed for Booking ID: {BookingId}", bookingId);
                }
            });
        }
    }
}