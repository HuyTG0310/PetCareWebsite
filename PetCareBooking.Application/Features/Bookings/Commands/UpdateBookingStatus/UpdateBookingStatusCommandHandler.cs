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
                // All items must be completed
                if (!booking.BookingItems.All(bi => bi.Status == BookingItemStatus.Completed))
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Cannot mark booking as completed. All items must be completed first.",
                        Result = Guid.Empty
                    };
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
            // Define valid state transitions for bookings
            var validTransitions = new Dictionary<BookingStatus, List<BookingStatus>>
            {
                {
                    BookingStatus.Pending,
                    new List<BookingStatus>
                    {
                        BookingStatus.Partially_Completed,
                        BookingStatus.Completed,
                        BookingStatus.Cancelled
                    }
                },
                {
                    BookingStatus.Partially_Completed,
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