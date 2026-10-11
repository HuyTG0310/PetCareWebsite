using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.DTOs.Payment;

namespace PetCareBooking.Application.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Guid bookingId, decimal amount, string orderDescription, string ipAddress, string? returnUrl = null);
        VnPayCallbackResult ProcessPaymentCallback(IQueryCollection query);
    }
}

