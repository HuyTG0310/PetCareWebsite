using MediatR;
using PetCareBooking.Application.Common.Models;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateVnPayPaymentUrl
{
    public class CreateVnPayPaymentUrlCommand : IRequest<ApiResponse<string>>
    {
        [JsonIgnore]
        public Guid BookingId { get; set; }

        public decimal? Amount { get; set; }
        public string? ReturnUrl { get; set; }
    }
}

