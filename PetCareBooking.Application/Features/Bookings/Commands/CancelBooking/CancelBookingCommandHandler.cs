using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CancelBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate booking exists
            var bookings = await _bookingRepository.FindAsync(b => b.Id == request.Id, "BookingItems");
            var booking = bookings.FirstOrDefault();

            if (booking == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            // Phân quyền: Customer chỉ được hủy booking của chính mình
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to cancel this booking.",
                        Result = Guid.Empty
                    };
                }
            }

            // 2. Validate booking can be cancelled based on status & role
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot cancel booking with status {booking.Status}.",
                    Result = Guid.Empty
                };
            }

            // Customer rule: Customer is ONLY allowed to cancel when status is Pending (Chưa xác nhận/chưa thanh toán)
            // Once Confirmed (Paid via VNPay or confirmed by receptionist), salon enforces No-Refund policy and blocks online cancellation
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (booking.Status != BookingStatus.Pending)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Đơn đặt lịch đã được xác nhận (Confirmed). Salon áp dụng chính sách không hoàn tiền và không hỗ trợ hủy online. Quý khách vui lòng liên hệ trực tiếp qua hotline để được hỗ trợ dời lịch hẹn.",
                        Result = Guid.Empty
                    };
                }
            }
            // Staff / Admin rule: Staff can cancel Pending or Confirmed, but NOT when pet has already Checked-In or service is in progress
            else
            {
                bool hasStartedItems = booking.BookingItems.Any(bi =>
                    bi.Status == BookingItemStatus.Checked_In ||
                    bi.Status == BookingItemStatus.In_Progress ||
                    bi.Status == BookingItemStatus.Completed);

                if (hasStartedItems)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Không thể hủy đơn đặt lịch khi thú cưng đã Check-in hoặc dịch vụ đang/đã được thực hiện.",
                        Result = Guid.Empty
                    };
                }
            }

            // 3. Cancel all booking items
            foreach (var item in booking.BookingItems)
            {
                item.Status = BookingItemStatus.Cancelled;
                _bookingItemRepository.Update(item);
            }

            // 4. Update booking status
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = request.CancellationReason;
            _bookingRepository.Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Booking cancelled successfully.",
                Result = booking.Id
            };
        }
    }
}
