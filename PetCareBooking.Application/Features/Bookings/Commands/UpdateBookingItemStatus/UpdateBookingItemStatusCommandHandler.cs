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
        private readonly IGenericRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBookingItemStatusCommandHandler(
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<Staff> staffRepository,
            IGenericRepository<Room> roomRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _bookingItemRepository = bookingItemRepository;
            _staffRepository = staffRepository;
            _roomRepository = roomRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _currentUserService = currentUserService;
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

            // 3. IDOR and Role Permission Check:
            bool isAdminOrReceptionist = _currentUserService.IsInRole("Admin") ||
                                         _currentUserService.IsInRole("Receptionist");

            if (!isAdminOrReceptionist)
            {
                // Grooming staff (or other staff) can only update tasks assigned to themselves
                if (bookingItem.StaffId.HasValue && _currentUserService.UserId.HasValue && bookingItem.StaffId.Value != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to update tasks assigned to another staff member.",
                        Result = Guid.Empty
                    };
                }

                // If not assigned yet and current staff starts the task, automatically claim/assign to current staff
                if (!bookingItem.StaffId.HasValue && _currentUserService.UserId.HasValue)
                {
                    bookingItem.StaffId = _currentUserService.UserId.Value;
                }

                // Regular staff cannot reassign room or change staff
                request.StaffId = bookingItem.StaffId;
                request.RoomId = bookingItem.RoomId;
            }

            // 4. Validate status transition is allowed (if status specified)
            var targetStatus = request.Status ?? bookingItem.Status;

            if (!IsValidStatusTransition(bookingItem.Status, targetStatus))
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot transition from {bookingItem.Status} to {targetStatus}.",
                    Result = Guid.Empty
                };
            }

            // 5. Validate staff if provided (Admin/Receptionist only)
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

            // 6. Validate room if provided (Admin/Receptionist only)
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

            // 7. Update booking item
            bookingItem.Status = targetStatus;
            if (request.StaffId.HasValue)
                bookingItem.StaffId = request.StaffId;
            if (request.RoomId.HasValue)
                bookingItem.RoomId = request.RoomId;
            if (!string.IsNullOrEmpty(request.ResultNote))
                bookingItem.ResultNote = request.ResultNote;
            if (!string.IsNullOrEmpty(request.BeforeImageUrl))
                bookingItem.BeforeImageUrl = request.BeforeImageUrl;
            if (!string.IsNullOrEmpty(request.ResultImageUrl))
                bookingItem.ResultImageUrl = request.ResultImageUrl;

            _bookingItemRepository.Update(bookingItem);

            // 8. Update booking status based on items and settlement
            await UpdateBookingStatusAsync(booking);

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
            if (from == to) return true; // Giữ nguyên trạng thái (khi Lễ tân gán nhân viên, xếp phòng...)
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

        private async Task UpdateBookingStatusAsync(Booking booking)
        {
            var itemStatuses = booking.BookingItems.Select(bi => bi.Status).ToList();

            if (itemStatuses.All(s => s == BookingItemStatus.Completed || s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show) &&
                itemStatuses.Any(s => s == BookingItemStatus.Completed))
            {
                var payments = await _paymentTransactionRepository.FindAsync(t => t.BookingId == booking.Id && t.PaymentStatus == "SUCCESS");
                var totalPaid = payments.Sum(t => t.Amount);

                // Nếu đã thanh toán đủ 100% (vd: VNPay trước đó) thì hoàn tất luôn đơn,
                // còn nếu chưa thanh toán đủ (Pay at counter) thì giữ Confirmed để Lễ tân Checkout thu tiền tại quầy.
                if (totalPaid >= booking.TotalPrice && booking.TotalPrice > 0)
                {
                    booking.Status = BookingStatus.Completed;
                }
                else
                {
                    booking.Status = BookingStatus.Confirmed;
                }
            }
            else if (itemStatuses.All(s => s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show))
            {
                booking.Status = BookingStatus.Cancelled;
            }
            else if (itemStatuses.Any(s => s == BookingItemStatus.Confirmed || s == BookingItemStatus.Checked_In || s == BookingItemStatus.In_Progress || s == BookingItemStatus.Completed))
            {
                booking.Status = BookingStatus.Confirmed;
            }
        }
    }
}