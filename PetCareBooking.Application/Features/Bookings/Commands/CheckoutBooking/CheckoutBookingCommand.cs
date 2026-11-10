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

        /// <summary>
        /// Payment method: CASH, BANK_TRANSFER, CARD, E_WALLET, VNPAY
        /// </summary>
        public string PaymentMethod { get; set; } = "CASH";

        /// <summary>
        /// Payment amount (if empty or 0, system automatically collects full remaining balance)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Reference code / bank authorization code (optional)
        /// </summary>
        public string? TransactionCode { get; set; }

        /// <summary>
        /// Payment notes from receptionist
        /// </summary>
        public string? Note { get; set; }
    }
}

