using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentsController(
            IVnPayService vnPayService,
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _vnPayService = vnPayService;
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Webhook / Callback endpoint tiếp nhận kết quả trả về từ cổng thanh toán VNPay
        /// </summary>
        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            var callbackResult = _vnPayService.ProcessPaymentCallback(Request.Query);

            if (!callbackResult.IsSuccess)
            {
                return Ok(new ApiResponse<VnPayCallbackResult>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = callbackResult.Message,
                    Result = callbackResult
                });
            }

            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == callbackResult.BookingId,
                "BookingItems"
            );
            var booking = bookings.FirstOrDefault();

            if (booking != null)
            {
                // Check for idempotent transaction processing (avoid duplicate payment records)
                var existingTxns = await _paymentTransactionRepository.FindAsync(
                    t => t.BookingId == booking.Id && t.TransactionCode == callbackResult.TransactionNo
                );

                if (!existingTxns.Any())
                {
                    var transaction = new PaymentTransaction
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        TransactionType = "PAYMENT",
                        Amount = callbackResult.Amount,
                        PaymentMethod = "VNPAY",
                        PaymentStatus = "SUCCESS",
                        TransactionCode = callbackResult.TransactionNo,
                        PaidAt = callbackResult.PayDate ?? DateTime.UtcNow,
                        Note = $"Thanh toan truc tuyen qua VNPAY (Ma ngan hang: {callbackResult.BankCode})",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _paymentTransactionRepository.AddAsync(transaction);

                    // Confirm booking and items if they were pending
                    booking.Status = BookingStatus.Confirmed;
                    foreach (var item in booking.BookingItems)
                    {
                        if (item.Status == BookingItemStatus.Pending)
                        {
                            item.Status = BookingItemStatus.Confirmed;
                            _bookingItemRepository.Update(item);
                        }
                    }

                    _bookingRepository.Update(booking);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                }
            }

            return Ok(new ApiResponse<VnPayCallbackResult>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "VNPay payment confirmed successfully.",
                Result = callbackResult
            });
        }
    }
}
