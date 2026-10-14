using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Payment;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetCheckoutSummary
{
    public class GetCheckoutSummaryQueryHandler : IRequestHandler<GetCheckoutSummaryQuery, ApiResponse<CheckoutSummaryResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCheckoutSummaryQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<CheckoutSummaryResponseDTO>> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == request.BookingId,
                "BookingItems.Pet", "BookingItems.Service", "BookingItems.Room", "BookingItems.Staff", "Customer", "Voucher"
            );

            var booking = bookings.FirstOrDefault();
            if (booking == null)
            {
                return new ApiResponse<CheckoutSummaryResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            // Customer IDOR check
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<CheckoutSummaryResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to access this booking checkout summary.",
                        Result = null
                    };
                }
            }

            // Fetch payment transactions
            var transactions = await _paymentTransactionRepository.FindAsync(
                t => t.BookingId == booking.Id
            );

            var successfulTransactions = transactions
                .Where(t => t.PaymentStatus == "SUCCESS")
                .ToList();

            var totalPaid = successfulTransactions.Sum(t => t.Amount);
            var remainingAmount = Math.Max(0, booking.TotalPrice - totalPaid);

            // Subtotal calculation
            var subTotal = booking.BookingItems.Sum(bi => bi.AssignedPrice);
            var discountAmount = Math.Max(0, subTotal - booking.TotalPrice);

            var paymentStatus = "UNPAID";
            if (totalPaid >= booking.TotalPrice && booking.TotalPrice > 0)
            {
                paymentStatus = "PAID";
            }
            else if (totalPaid > 0)
            {
                paymentStatus = "PARTIALLY_PAID";
            }

            var hasUncompletedItems = booking.BookingItems.Any(bi =>
                bi.Status != BookingItemStatus.Completed &&
                bi.Status != BookingItemStatus.Cancelled &&
                bi.Status != BookingItemStatus.No_Show);

            var canCheckout = booking.Status != BookingStatus.Cancelled &&
                              !hasUncompletedItems &&
                              (remainingAmount > 0 || booking.Status != BookingStatus.Completed);

            string? restrictionReason = null;
            if (booking.Status == BookingStatus.Cancelled)
            {
                restrictionReason = "Đơn đặt lịch đã bị hủy.";
            }
            else if (hasUncompletedItems)
            {
                restrictionReason = "Vẫn còn dịch vụ chưa hoàn thành hoặc đang được thực hiện. Vui lòng chờ nhân viên hoàn tất trước khi Check-out.";
            }
            else if (booking.Status == BookingStatus.Completed && remainingAmount <= 0)
            {
                restrictionReason = "Đơn đặt lịch đã hoàn tất và thanh toán đầy đủ.";
            }

            var response = new CheckoutSummaryResponseDTO
            {
                BookingId = booking.Id,
                BookingStatus = booking.Status,
                BookingCreatedAt = booking.CreatedAt,
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer.FullName,
                CustomerPhone = booking.Customer.PhoneNumber,
                CustomerEmail = booking.Customer.Email,
                SubTotal = subTotal,
                VoucherId = booking.VoucherId,
                VoucherCode = booking.Voucher?.Code,
                DiscountAmount = discountAmount,
                TotalPrice = booking.TotalPrice,
                TotalPaid = totalPaid,
                RemainingAmount = remainingAmount,
                PaymentStatus = paymentStatus,
                CanCheckout = canCheckout,
                RestrictionReason = restrictionReason,
                Items = booking.BookingItems.Select(bi => new CheckoutItemSummaryDTO
                {
                    ItemId = bi.Id,
                    PetId = bi.PetId,
                    PetName = bi.Pet.Name,
                    ServiceId = bi.ServiceId,
                    ServiceName = bi.Service.Name,
                    ServiceType = bi.Service.ServiceType,
                    RoomId = bi.RoomId,
                    RoomName = bi.Room?.RoomName,
                    StaffId = bi.StaffId,
                    StaffName = bi.Staff?.FullName,
                    Quantity = bi.Quantity,
                    UnitPrice = bi.UnitPrice,
                    AssignedPrice = bi.AssignedPrice,
                    Status = bi.Status,
                    ScheduledStartAt = bi.ScheduledStartAt,
                    ScheduledEndAt = bi.ScheduledEndAt
                }).ToList(),
                PaymentHistory = transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new PaymentTransactionResponseDTO
                    {
                        Id = t.Id,
                        BookingId = t.BookingId,
                        TransactionType = t.TransactionType,
                        Amount = t.Amount,
                        PaymentMethod = t.PaymentMethod,
                        PaymentStatus = t.PaymentStatus,
                        TransactionCode = t.TransactionCode,
                        PaidAt = t.PaidAt,
                        Note = t.Note,
                        CreatedAt = t.CreatedAt
                    }).ToList()
            };

            return new ApiResponse<CheckoutSummaryResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get checkout summary successfully.",
                Result = response
            };
        }
    }
}

