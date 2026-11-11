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
                    Message = "Can not mark No_show because pet is being performed service",
                    Result = Guid.Empty
                };
            }

            foreach (var item in booking.BookingItems)
            {
                if (item.Status != BookingItemStatus.Cancelled)
                {
                    item.Status = BookingItemStatus.No_Show;
                    item.ResultNote = string.IsNullOrWhiteSpace(request.Reason)
                        ? "Customer did not show up (No-Show)"
                        : request.Reason.Trim();
                    _bookingItemRepository.Update(item);
                }
            }

            booking.Status = BookingStatus.No_Show;
            booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Customer did not show up (No-Show)"
                : request.Reason.Trim();

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Booking was marked as No_show. Booking and rooms were released successfully",
                Result = booking.Id
            };
        }
    }
}

