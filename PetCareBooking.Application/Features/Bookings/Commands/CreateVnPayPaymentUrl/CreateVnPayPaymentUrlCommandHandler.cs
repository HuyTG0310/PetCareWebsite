using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.CreateVnPayPaymentUrl
{
    public class CreateVnPayPaymentUrlCommandHandler : IRequestHandler<CreateVnPayPaymentUrlCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        public CreateVnPayPaymentUrlCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IVnPayService vnPayService,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<string>> Handle(CreateVnPayPaymentUrlCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<string>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to pay for this booking.",
                        Result = null
                    };
                }
            }

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.No_Show)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot pay for a booking with status {booking.Status}.",
                    Result = null
                };
            }

            var transactions = await _paymentTransactionRepository.FindAsync(
                t => t.BookingId == booking.Id && t.PaymentStatus == "SUCCESS"
            );
            var totalPaid = transactions.Sum(t => t.Amount);
            var remainingAmount = Math.Max(0, booking.TotalPrice - totalPaid);

            if (remainingAmount <= 0)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Booking is already fully paid.",
                    Result = null
                };
            }

            var payAmount = (request.Amount.HasValue && request.Amount.Value > 0)
                ? Math.Min(request.Amount.Value, remainingAmount)
                : remainingAmount;

            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(
                booking.Id,
                payAmount,
                $"Thanh toan don {booking.Id}",
                ipAddress,
                request.ReturnUrl
            );

            return new ApiResponse<string>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "VNPay payment URL generated successfully.",
                Result = paymentUrl
            };
        }
    }
}

