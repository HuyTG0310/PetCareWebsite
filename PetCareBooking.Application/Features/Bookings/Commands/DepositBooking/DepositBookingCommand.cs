using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using System.Text.Json.Serialization;

namespace PetCareBooking.Application.Features.Bookings.Commands.DepositBooking
{
    public class DepositBookingCommand : IRequest<ApiResponse<PaymentTransactionResponseDTO>>
    {
        [JsonIgnore]
        public Guid BookingId { get; set; }

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "BANK_TRANSFER";
        public string? TransactionCode { get; set; }
        public string? Note { get; set; }
    }
}

