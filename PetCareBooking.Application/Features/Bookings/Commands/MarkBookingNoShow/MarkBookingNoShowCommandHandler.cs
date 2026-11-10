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
            // 1. Verify booking exists along with list of service items (BookingItems)
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

            // 2. Check booking status: Cannot mark as No-Show if booking is completed, cancelled, or previously marked as No-Show
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

            // 3. Business constraint: Pet has not been checked in and service has not started
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

            // 4. Update all unstarted service items to No_Show status
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

            // 5. Update Booking status to No_Show with clear reason
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

