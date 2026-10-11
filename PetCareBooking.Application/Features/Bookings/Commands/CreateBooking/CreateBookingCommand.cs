using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<ApiResponse<CreateBookingResponseDTO>>
    {
        public Guid? CustomerId { get; set; }
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Lựa chọn thanh toán: PAY_AT_COUNTER (Thanh toán tại quầy khi đến tiệm) hoặc VNPAY (Thanh toán online qua cổng VNPAY)
        /// </summary>
        public string PaymentChoice { get; set; } = "PAY_AT_COUNTER";

        public List<BookingItemRequestDTO> BookingItems { get; set; } = new List<BookingItemRequestDTO>();
    }
}