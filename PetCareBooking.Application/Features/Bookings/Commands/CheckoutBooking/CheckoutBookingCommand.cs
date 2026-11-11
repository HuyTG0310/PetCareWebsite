using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckoutBooking
{
    public class CheckoutBookingCommand : IRequest<ApiResponse<CheckoutResponseDTO>>
    {
        [JsonIgnore]
        public Guid BookingId { get; set; }

        public string PaymentMethod { get; set; } = "CASH";

        public decimal? Amount { get; set; }

        public string? TransactionCode { get; set; }

        public string? Note { get; set; }
    }
}

