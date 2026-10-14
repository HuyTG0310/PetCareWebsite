using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBookingStatusCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
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

            // 2. Validate status transition is allowed
            if (!IsValidStatusTransition(booking.Status, request.Status))
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot transition booking from {booking.Status} to {request.Status}.",
                    Result = Guid.Empty
                };
            }

            // 3. Validate business rules based on new status
            if (request.Status == BookingStatus.Completed)
            {
                // All items must be completed or cancelled
                if (!booking.BookingItems.All(bi => bi.Status == BookingItemStatus.Completed || bi.Status == BookingItemStatus.Cancelled || bi.Status == BookingItemStatus.No_Show))
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Cannot mark booking as completed. All active items must be completed first.",
                        Result = Guid.Empty
                    };
                }
            }

            if (request.Status == BookingStatus.Cancelled)
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

                foreach (var item in booking.BookingItems)
                {
                    item.Status = BookingItemStatus.Cancelled;
                }
            }

            // 4. Update booking status
            booking.Status = request.Status;
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Booking status updated successfully.",
                Result = booking.Id
            };
        }

        private bool IsValidStatusTransition(BookingStatus from, BookingStatus to)
        {
            if (from == to) return true;

            // Define valid state transitions for bookings
            var validTransitions = new Dictionary<BookingStatus, List<BookingStatus>>
            {
                {
                    BookingStatus.Pending,
                    new List<BookingStatus>
                    {
                        BookingStatus.Confirmed,
                        BookingStatus.Completed,
                        BookingStatus.Cancelled
                    }
                },
                {
                    BookingStatus.Confirmed,
                    new List<BookingStatus>
                    {
                        BookingStatus.Completed,
                        BookingStatus.Cancelled
                    }
                },
                {
                    BookingStatus.Completed,
                    new List<BookingStatus> { } // Terminal state
                },
                {
                    BookingStatus.Cancelled,
                    new List<BookingStatus> { } // Terminal state
                }
            };

            return validTransitions.ContainsKey(from) && validTransitions[from].Contains(to);
        }
    }
}