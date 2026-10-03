using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Commands.UpdateBookingItemStatus
{
    public class UpdateBookingItemStatusCommandHandler : IRequestHandler<UpdateBookingItemStatusCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<Staff> _staffRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBookingItemStatusCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<Staff> staffRepository,
            IGenericRepository<Room> roomRepository,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _staffRepository = staffRepository;
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateBookingItemStatusCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate booking exists
            var bookings = await _bookingRepository.FindAsync(b => b.Id == request.BookingId, "BookingItems");
            var booking = bookings.FirstOrDefault();

            if (booking == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking with ID {request.BookingId} not found.",
                    Result = Guid.Empty
                };
            }

            // 2. Validate booking item exists
            var bookingItem = booking.BookingItems.FirstOrDefault(bi => bi.Id == request.ItemId);

            if (bookingItem == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Booking item with ID {request.ItemId} not found.",
                    Result = Guid.Empty
                };
            }

            // 3. Validate status transition is allowed
            if (!IsValidStatusTransition(bookingItem.Status, request.Status))
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot transition from {bookingItem.Status} to {request.Status}.",
                    Result = Guid.Empty
                };
            }

            // 4. Validate staff if provided
            if (request.StaffId.HasValue)
            {
                var staff = (await _staffRepository.FindAsync(s => s.Id == request.StaffId)).FirstOrDefault();
                if (staff == null)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Staff with ID {request.StaffId} not found.",
                        Result = Guid.Empty
                    };
                }
            }

            // 5. Validate room if provided
            if (request.RoomId.HasValue)
            {
                var room = (await _roomRepository.FindAsync(r => r.Id == request.RoomId)).FirstOrDefault();
                if (room == null)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Room with ID {request.RoomId} not found.",
                        Result = Guid.Empty
                    };
                }
            }

            // 6. Update booking item
            bookingItem.Status = request.Status;
            if (request.StaffId.HasValue)
                bookingItem.StaffId = request.StaffId;
            if (request.RoomId.HasValue)
                bookingItem.RoomId = request.RoomId;
            if (!string.IsNullOrEmpty(request.ResultNote))
                bookingItem.ResultNote = request.ResultNote;
            if (!string.IsNullOrEmpty(request.ResultImageUrl))
                bookingItem.ResultImageUrl = request.ResultImageUrl;

            _bookingItemRepository.Update(bookingItem);

            // 7. Update booking status based on items
            UpdateBookingStatus(booking);

            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Booking item status updated successfully.",
                Result = bookingItem.Id
            };
        }

        private bool IsValidStatusTransition(BookingItemStatus from, BookingItemStatus to)
        {
            var validTransitions = new Dictionary<BookingItemStatus, List<BookingItemStatus>>
            {
                { BookingItemStatus.Pending, new List<BookingItemStatus> { BookingItemStatus.Confirmed, BookingItemStatus.Cancelled } },
                { BookingItemStatus.Confirmed, new List<BookingItemStatus> { BookingItemStatus.Checked_In, BookingItemStatus.Cancelled } },
                { BookingItemStatus.Checked_In, new List<BookingItemStatus> { BookingItemStatus.In_Progress, BookingItemStatus.No_Show } },
                { BookingItemStatus.In_Progress, new List<BookingItemStatus> { BookingItemStatus.Completed, BookingItemStatus.Cancelled } },
                { BookingItemStatus.Completed, new List<BookingItemStatus> { } },
                { BookingItemStatus.Cancelled, new List<BookingItemStatus> { } },
                { BookingItemStatus.No_Show, new List<BookingItemStatus> { } }
            };

            return validTransitions.ContainsKey(from) && validTransitions[from].Contains(to);
        }

        private void UpdateBookingStatus(Booking booking)
        {
            var itemStatuses = booking.BookingItems.Select(bi => bi.Status).ToList();

            if (itemStatuses.All(s => s == BookingItemStatus.Completed))
            {
                booking.Status = BookingStatus.Completed;
            }
            else if (itemStatuses.Any(s => s == BookingItemStatus.Completed))
            {
                booking.Status = BookingStatus.Partially_Completed;
            }
            else if (itemStatuses.Any(s => s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show))
            {
                if (itemStatuses.All(s => s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show))
                {
                    booking.Status = BookingStatus.Cancelled;
                }
            }
        }
    }
}