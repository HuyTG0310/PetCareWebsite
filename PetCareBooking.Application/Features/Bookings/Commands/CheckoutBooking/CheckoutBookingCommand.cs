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
        /// Phương thức thanh toán: CASH, BANK_TRANSFER, CARD, E_WALLET, VNPAY
        /// </summary>
        public string PaymentMethod { get; set; } = "CASH";

        /// <summary>
        /// Số tiền thanh toán (nếu để trống hoặc 0, hệ thống tự động thu toàn bộ số tiền còn lại)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Mã tham chiếu / mã chuẩn chi ngân hàng (tùy chọn)
        /// </summary>
        public string? TransactionCode { get; set; }

        /// <summary>
        /// Ghi chú thanh toán của lễ tân
        /// </summary>
        public string? Note { get; set; }
    }
}

