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
            var bookings = await _bookingRepository.FindAsync(b => b.Id == request.BookingId, "BookingItems.Service");
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

            // 1.1 Validate booking is not already completed (checked out), cancelled, or no-show
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.No_Show)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Can not assign staffs for checked-out booking, or cancelled or no show",
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

            // Re-check staff assignment against item's existing staffId
            if (request.StaffId.HasValue && request.StaffId != bookingItem.StaffId && booking.Status != BookingStatus.Confirmed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Only can assign staffs after booking was Confirmed",
                    Result = Guid.Empty
                };
            }

            // Diet services are parts of Boarding service and will not be assign specific rooms
            if (bookingItem.Service != null && bookingItem.Service.ServiceType == ServiceType.Diet)
            {
                request.RoomId = null; // Diet không được gán phòng riêng
            }

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
                    if (booking.Status != BookingStatus.Confirmed)
                    {
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Can only take the tasks after booking is confirmed.",
                            Result = Guid.Empty
                        };
                    }
                    bookingItem.StaffId = _currentUserService.UserId.Value;
                }

                // Regular staff cannot reassign room or change staff
                request.StaffId = bookingItem.StaffId;
                request.RoomId = bookingItem.RoomId;
            }

            // 4. Validate status transition is allowed (if status specified)
            var targetStatus = request.Status ?? bookingItem.Status;

            // Prevent check-in or perform services if booking is not Confirmed
            if ((targetStatus == BookingItemStatus.Checked_In || targetStatus == BookingItemStatus.In_Progress) && booking.Status != BookingStatus.Confirmed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Can only check-in or perform services for Confirmed booking (Confirmed).",
                    Result = Guid.Empty
                };
            }

            // Must have staffs who are responsible for services when booking is in Checked_In or In_Progress status
            if ((targetStatus == BookingItemStatus.Checked_In || targetStatus == BookingItemStatus.In_Progress) &&
                !bookingItem.StaffId.HasValue && !request.StaffId.HasValue)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Staff ID is required when status is Checked_In or In_Progress.",
                    Result = Guid.Empty
                };
            }

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

            if (request.Status.HasValue && isAdminOrReceptionist && !_currentUserService.HasPermission("BOOKING_UPDATE_STATUS"))
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "You have no permissions to update service progress (Require permission BOOKING_UPDATE_STATUS).",
                    Result = Guid.Empty
                };
            }

            if ((request.StaffId.HasValue || request.RoomId.HasValue) && isAdminOrReceptionist && !_currentUserService.HasPermission("BOOKING_ASSIGN_STAFF"))
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "You have no permission to assign staffs or roooms (Require permission BOOKING_ASSIGN_STAFF).",
                    Result = Guid.Empty
                };
            }

            // 5. Validate staff if provided (Admin/Receptionist only)
            if (request.StaffId.HasValue)
            {
                var staff = (await _staffRepository.FindAsync(s => s.Id == request.StaffId, "StaffRoles.Role")).FirstOrDefault();
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

                if (staff.Status != StaffStatus.Active)
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Staff '{staff.FullName}' is currently in Inactive status.",
                        Result = Guid.Empty
                    };
                }

                var staffRoleNames = staff.StaffRoles.Select(sr => sr.Role.RoleName).ToList();

                // Do not allow assigning Admin without technical staff roles
                if (staffRoleNames.Contains("Admin") && !staffRoleNames.Contains("Grooming Staff") && !staffRoleNames.Contains("Boarding Staff"))
                {
                    return new ApiResponse<Guid>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Can not assign Admin account to perform service.",
                        Result = Guid.Empty
                    };
                }

                if (bookingItem.Service != null)
                {
                    if (bookingItem.Service.ServiceType == ServiceType.Boarding)
                    {
                        if (!staffRoleNames.Any(r => r.Contains("Boarding", StringComparison.OrdinalIgnoreCase)))
                        {
                            return new ApiResponse<Guid>
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = $"Staff '{staff.FullName}' is not belong to Boarding department to perform service '{bookingItem.Service.Name}'.",
                                Result = Guid.Empty
                            };
                        }
                    }
                    else if (bookingItem.Service.ServiceType == ServiceType.Grooming || bookingItem.Service.ServiceType == ServiceType.Care)
                    {
                        if (!staffRoleNames.Any(r => r.Contains("Grooming", StringComparison.OrdinalIgnoreCase)))
                        {
                            return new ApiResponse<Guid>
                            {
                                IsSuccess = false,
                                StatusCode = 400,
                                Message = $"Staff '{staff.FullName}' is not belong to Grooming department to perform service '{bookingItem.Service.Name}'.",
                                Result = Guid.Empty
                            };
                        }
                    }
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

            // Automatically synchning for Diet service
            if (bookingItem.Service != null && bookingItem.Service.ServiceType == ServiceType.Boarding)
            {
                var accompanyingDietItem = booking.BookingItems.FirstOrDefault(bi =>
                    bi.Id != bookingItem.Id &&
                    bi.PetId == bookingItem.PetId &&
                    bi.Service != null &&
                    bi.Service.ServiceType == ServiceType.Diet);

                if (accompanyingDietItem != null)
                {
                    accompanyingDietItem.StaffId = bookingItem.StaffId;
                    accompanyingDietItem.RoomId = bookingItem.RoomId;
                    accompanyingDietItem.Status = targetStatus;
                    _bookingItemRepository.Update(accompanyingDietItem);
                }
            }

            // 8. Update booking status based on items
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
            if (from == to) return true;
            var validTransitions = new Dictionary<BookingItemStatus, List<BookingItemStatus>>
            {
                { BookingItemStatus.Pending, new List<BookingItemStatus> { BookingItemStatus.Confirmed, BookingItemStatus.Cancelled, BookingItemStatus.No_Show } },
                { BookingItemStatus.Confirmed, new List<BookingItemStatus> { BookingItemStatus.Checked_In, BookingItemStatus.Cancelled, BookingItemStatus.No_Show } },
                { BookingItemStatus.Checked_In, new List<BookingItemStatus> { BookingItemStatus.In_Progress, BookingItemStatus.Cancelled } },
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

            // if all services are No-Show -> booking will be No_Show
            if (itemStatuses.All(s => s == BookingItemStatus.No_Show))
            {
                booking.Status = BookingStatus.No_Show;
                if (string.IsNullOrEmpty(booking.CancellationReason))
                {
                    booking.CancellationReason = "No show";
                }
            }
            // if all service are cancelled -> booking will be Cancelled
            else if (itemStatuses.All(s => s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show))
            {
                booking.Status = BookingStatus.Cancelled;
            }
            else if (booking.Status == BookingStatus.Confirmed && itemStatuses.Any(s => s == BookingItemStatus.Confirmed || s == BookingItemStatus.Checked_In || s == BookingItemStatus.In_Progress || s == BookingItemStatus.Completed))
            {
                booking.Status = BookingStatus.Confirmed;
            }
        }
    }
}
