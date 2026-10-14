using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.CheckInBooking
{
    public class CheckInBookingCommandHandler : IRequestHandler<CheckInBookingCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CheckInBookingCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CheckInBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate booking exists
            var bookings = await _bookingRepository.FindAsync(b => b.Id == request.Id, "BookingItems.Service");
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

            // 2. Validate booking status must be Confirmed
            if (booking.Status != BookingStatus.Confirmed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Chỉ có thể Check-in cho đơn đặt lịch đã được Xác nhận (Confirmed). Vui lòng xác nhận đơn và phân công đầy đủ trước khi Check-in.",
                    Result = Guid.Empty
                };
            }

            // 3. Find eligible items to check in (Pending or Confirmed)
            var eligibleItems = booking.BookingItems
                .Where(bi => bi.Status == BookingItemStatus.Pending || bi.Status == BookingItemStatus.Confirmed)
                .ToList();

            if (!eligibleItems.Any())
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "No pending or confirmed items found to check-in.",
                    Result = booking.Id
                };
            }

            // 4. Validate that all eligible items are fully assigned before check-in
            var unassignedItems = eligibleItems.Where(item =>
            {
                bool isBoarding = item.Service != null && item.Service.ServiceType == ServiceType.Boarding;
                bool isDiet = item.Service != null && item.Service.ServiceType == ServiceType.Diet;
                if (isDiet) return false; // Diet inherits boarding
                if (isBoarding)
                {
                    return !item.StaffId.HasValue || !item.RoomId.HasValue;
                }
                // Grooming and other services
                return !item.StaffId.HasValue;
            }).ToList();

            if (unassignedItems.Any())
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Không thể Check-in. Vui lòng phân công nhân viên và xếp phòng đầy đủ cho tất cả dịch vụ trước khi Check-in.",
                    Result = Guid.Empty
                };
            }

            // 5. Update all eligible items to Checked_In
            foreach (var item in eligibleItems)
            {
                item.Status = BookingItemStatus.Checked_In;
                _bookingItemRepository.Update(item);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Check-in successfully for {eligibleItems.Count} item(s) in this booking.",
                Result = booking.Id
            };
        }
    }
}
