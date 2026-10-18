using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.MarkBookingNoShow
{
    public class MarkBookingNoShowCommandHandler : IRequestHandler<MarkBookingNoShowCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkBookingNoShowCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(MarkBookingNoShowCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra đơn đặt lịch tồn tại kèm danh sách các dịch vụ (BookingItems)
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

            // 2. Kiểm tra trạng thái đơn: Không thể đánh dấu No-Show nếu đơn đã hoàn thành, đã bị hủy hoặc đã đánh dấu No-Show trước đó
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.No_Show)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot mark booking as No-Show with current status: {booking.Status}.",
                    Result = Guid.Empty
                };
            }

            // 3. Ràng buộc nghiệp vụ: Thú cưng chưa được Check-in và dịch vụ chưa bắt đầu thực hiện
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
                    Message = "Không thể đánh dấu No-Show vì thú cưng đã Check-in hoặc dịch vụ đang/đã được thực hiện.",
                    Result = Guid.Empty
                };
            }

            // 4. Cập nhật toàn bộ các dịch vụ chưa bắt đầu sang trạng thái No_Show
            foreach (var item in booking.BookingItems)
            {
                if (item.Status != BookingItemStatus.Cancelled)
                {
                    item.Status = BookingItemStatus.No_Show;
                    item.ResultNote = string.IsNullOrWhiteSpace(request.Reason)
                        ? "Khách không đến theo lịch hẹn (No-Show)"
                        : request.Reason.Trim();
                    _bookingItemRepository.Update(item);
                }
            }

            // 5. Cập nhật trạng thái Booking sang No_Show với lý do rõ ràng
            booking.Status = BookingStatus.No_Show;
            booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Khách không đến theo lịch hẹn (No-Show)"
                : request.Reason.Trim();

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Đơn đặt lịch đã được đánh dấu là Khách không đến (No-Show). Lịch hẹn và phòng đã được giải phóng thành công.",
                Result = booking.Id
            };
        }
    }
}

