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

            // 2. Validate booking is not in a terminal state
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot check-in booking with status {booking.Status}.",
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

            // 4. Update all eligible items to Checked_In
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
