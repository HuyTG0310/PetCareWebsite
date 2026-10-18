using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode, CancellationToken cancellationToken = default);
        Task SendBookingConfirmationEmailAsync(Booking booking, CancellationToken cancellationToken = default);
        void QueueBookingConfirmationEmail(Guid bookingId);
    }
}