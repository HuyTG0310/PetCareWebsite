using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetBookingReceipt
{
    public class GetBookingReceiptQueryHandler : IRequestHandler<GetBookingReceiptQuery, ApiResponse<BookingReceiptResponseDTO>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly IGenericRepository<Staff> _staffRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetBookingReceiptQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            IGenericRepository<Staff> staffRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _staffRepository = staffRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<BookingReceiptResponseDTO>> Handle(GetBookingReceiptQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.FindAsync(
                b => b.Id == request.BookingId,
                "BookingItems.Pet",
                "BookingItems.Service",
                "BookingItems.Room.RoomType",
                "BookingItems.Staff",
                "Customer",
                "Voucher"
            );

            var booking = bookings.FirstOrDefault();
            if (booking == null)
            {
                return new ApiResponse<BookingReceiptResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = null
                };
            }

            // Chống IDOR: Khách hàng chỉ được xem biên lai đơn hàng của chính mình
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<BookingReceiptResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Bạn không có quyền truy cập biên lai của đơn đặt lịch này.",
                        Result = null
                    };
                }
            }

            // Lấy danh sách giao dịch thanh toán
            var transactions = await _paymentTransactionRepository.FindAsync(
                t => t.BookingId == booking.Id
            );

            var successfulTransactions = transactions
                .Where(t => t.PaymentStatus == "SUCCESS")
                .OrderBy(t => t.PaidAt ?? t.CreatedAt)
                .ToList();

            // Tính toán tài chính
            var subTotal = booking.BookingItems.Sum(bi => bi.AssignedPrice);
            var discountAmount = Math.Max(0, subTotal - booking.TotalPrice);
            var totalPaid = successfulTransactions.Sum(t => t.Amount);
            var remainingBalance = Math.Max(0, booking.TotalPrice - totalPaid);

            var paymentStatus = "UNPAID";
            if (totalPaid >= booking.TotalPrice && booking.TotalPrice > 0)
            {
                paymentStatus = "PAID";
            }
            else if (totalPaid > 0)
            {
                paymentStatus = "PARTIALLY_PAID";
            }

            // Xác định phương thức thanh toán chính
            var distinctMethods = successfulTransactions
                .Select(t => t.PaymentMethod.Trim().ToUpper())
                .Distinct()
                .ToList();

            var primaryPaymentMethod = distinctMethods.Count switch
            {
                0 => "CHƯA THANH TOÁN",
                1 => distinctMethods[0],
                _ => string.Join(" + ", distinctMethods)
            };

            // Xác định tên thu ngân / lễ tân xuất biên lai
            string cashierName = "Thu ngân quầy Lễ tân";
            if (_currentUserService.IsAdminOrStaff && _currentUserService.UserId.HasValue)
            {
                var staff = await _staffRepository.GetByIdAsync(_currentUserService.UserId.Value);
                if (staff != null && !string.IsNullOrWhiteSpace(staff.FullName))
                {
                    cashierName = staff.FullName;
                }
            }
            else
            {
                var lastStaffTxn = successfulTransactions.LastOrDefault(t => !string.IsNullOrEmpty(t.Note));
                if (lastStaffTxn != null && lastStaffTxn.PaymentMethod != "VNPAY")
                {
                    cashierName = "Lễ tân Pet Care";
                }
            }

            // Thời gian hoàn tất
            DateTime? completedAt = null;
            if (booking.Status == BookingStatus.Completed)
            {
                completedAt = successfulTransactions.LastOrDefault()?.PaidAt ?? booking.UpdatedAt ?? DateTime.UtcNow;
            }

            // Mã biên lai
            var receiptNo = $"REC-{booking.CreatedAt:yyyyMMdd}-{booking.Id.ToString()[..8].ToUpper()}";

            // Tổng hợp thông tin thú cưng
            var pets = booking.BookingItems
                .Where(bi => bi.Pet != null)
                .Select(bi => bi.Pet)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .Select(p => new ReceiptPetSummaryDTO
                {
                    PetId = p.Id,
                    Name = p.Name,
                    Species = p.Species.ToString(),
                    Breed = p.Breed,
                    Weight = p.Weight
                })
                .ToList();

            // Danh sách các dịch vụ chi tiết
            var items = booking.BookingItems
                .OrderBy(bi => bi.ScheduledStartAt)
                .Select((bi, index) =>
                {
                    string? roomInfo = null;
                    if (bi.Room != null)
                    {
                        roomInfo = bi.Room.RoomType != null
                            ? $"{bi.Room.RoomName} ({bi.Room.RoomType.Name})"
                            : bi.Room.RoomName;
                    }

                    return new ReceiptItemDTO
                    {
                        ItemIndex = index + 1,
                        ItemId = bi.Id,
                        PetId = bi.PetId,
                        PetName = bi.Pet?.Name ?? "Thú cưng",
                        ServiceName = bi.Service?.Name ?? "Dịch vụ",
                        ServiceType = bi.Service != null ? bi.Service.ServiceType.ToString() : "Grooming",
                        RoomInfo = roomInfo,
                        StaffName = bi.Staff?.FullName,
                        ScheduledStartAt = bi.ScheduledStartAt,
                        ScheduledEndAt = bi.ScheduledEndAt,
                        Quantity = bi.Quantity,
                        UnitPrice = bi.UnitPrice,
                        TotalPrice = bi.AssignedPrice,
                        Status = bi.Status.ToString(),
                        ResultNote = bi.ResultNote
                    };
                })
                .ToList();

            // Lịch sử thanh toán
            var paymentHistory = transactions
                .OrderBy(t => t.CreatedAt)
                .Select(t => new ReceiptPaymentTransactionDTO
                {
                    TransactionId = t.Id,
                    TransactionCode = t.TransactionCode,
                    TransactionType = t.TransactionType,
                    PaymentMethod = t.PaymentMethod,
                    Amount = t.Amount,
                    PaidAt = t.PaidAt,
                    PaymentStatus = t.PaymentStatus,
                    Note = t.Note
                })
                .ToList();

            var receiptDto = new BookingReceiptResponseDTO
            {
                Store = new StoreInfoDTO(),
                ReceiptNo = receiptNo,
                BookingId = booking.Id,
                PrintedAt = DateTime.UtcNow,
                BookingCreatedAt = booking.CreatedAt,
                CompletedAt = completedAt,
                BookingStatus = booking.Status.ToString(),
                CashierName = cashierName,
                Customer = new ReceiptCustomerDTO
                {
                    CustomerId = booking.CustomerId,
                    FullName = booking.Customer?.FullName ?? "Khách vãng lai",
                    PhoneNumber = booking.Customer?.PhoneNumber ?? "N/A",
                    Email = booking.Customer?.Email ?? "N/A",
                    Address = booking.Customer?.Address
                },
                Pets = pets,
                Items = items,
                SubTotal = subTotal,
                VoucherId = booking.VoucherId,
                VoucherCode = booking.Voucher?.Code,
                DiscountAmount = discountAmount,
                TotalPrice = booking.TotalPrice,
                TotalPaid = totalPaid,
                RemainingBalance = remainingBalance,
                PaymentStatus = paymentStatus,
                PrimaryPaymentMethod = primaryPaymentMethod,
                PaymentHistory = paymentHistory,
                FooterNote = "Cảm ơn Quý khách và các bé cưng đã sử dụng dịch vụ tại Pet Care! Quý khách vui lòng kiểm tra hóa đơn và tình trạng của bé trước khi rời quầy.",
                HotlineSupport = "1900 8888 / 0901 234 567"
            };

            return new ApiResponse<BookingReceiptResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Xuất biên lai thanh toán thành công.",
                Result = receiptDto
            };
        }
    }
}

