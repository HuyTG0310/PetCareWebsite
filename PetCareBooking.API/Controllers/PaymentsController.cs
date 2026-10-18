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
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IVnPayService vnPayService,
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<PaymentsController> logger)
        {
            _vnPayService = vnPayService;
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
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
                "BookingItems.Pet", "BookingItems.Service", "BookingItems.Room", "Customer", "PaymentTransactions", "Voucher"
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

                    // Confirm items if they were pending
                    foreach (var item in booking.BookingItems)
                    {
                        if (item.Status == BookingItemStatus.Pending)
                        {
                            item.Status = BookingItemStatus.Confirmed;
                            _bookingItemRepository.Update(item);
                        }
                    }

                    bool wasPending = (booking.Status == BookingStatus.Pending);

                    // If booking was pending, confirm it upon payment
                    if (wasPending)
                    {
                        booking.Status = BookingStatus.Confirmed;
                    }

                    _bookingRepository.Update(booking);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    // Send confirmation email if booking just became Confirmed
                    if (wasPending)
                    {
                        try
                        {
                            booking.PaymentTransactions.Add(transaction);
                            await _emailService.SendBookingConfirmationEmailAsync(booking);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send confirmation email after VNPay payment for Booking ID: {BookingId}", booking.Id);
                        }
                    }
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
