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
                    Message = "Không thể chỉnh sửa thông tin hoặc phân công nhân viên/phòng cho đơn đặt lịch đã hoàn thành (Check-out), đã hủy hoặc đã No-Show.",
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
                    Message = "Chỉ có thể phân công nhân viên sau khi đơn đặt lịch đã được Xác nhận (Confirmed).",
                    Result = Guid.Empty
                };
            }

            // Quy tắc nghiệp vụ: Dịch vụ Diet là option đi kèm với dịch vụ Boarding, không gán phòng riêng
            if (bookingItem.Service != null && bookingItem.Service.ServiceType == ServiceType.Diet)
            {
                request.RoomId = null; // Diet không được gán phòng riêng
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
                    if (booking.Status != BookingStatus.Confirmed)
                    {
                        return new ApiResponse<Guid>
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Chỉ có thể nhận việc sau khi đơn đặt lịch đã được Xác nhận (Confirmed).",
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

            // Chặn check-in hoặc thực hiện dịch vụ nếu booking chưa Confirmed
            if ((targetStatus == BookingItemStatus.Checked_In || targetStatus == BookingItemStatus.In_Progress) && booking.Status != BookingStatus.Confirmed)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Chỉ có thể Check-in hoặc thực hiện dịch vụ khi đơn đặt lịch đã được Xác nhận (Confirmed).",
                    Result = Guid.Empty
                };
            }

            // Bắt buộc phải có nhân viên phụ trách khi Checked_In hoặc In_Progress
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
                        Message = $"Nhân viên '{staff.FullName}' hiện đang ở trạng thái ngưng hoạt động (Inactive).",
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
                        Message = "Không thể phân công tài khoản Quản trị viên (Admin) làm nhân viên thực hiện dịch vụ.",
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
                                Message = $"Nhân viên '{staff.FullName}' không thuộc bộ phận Chăm sóc lưu trú (Boarding Staff) để phụ trách dịch vụ '{bookingItem.Service.Name}'.",
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
                                Message = $"Nhân viên '{staff.FullName}' không thuộc bộ phận Cắt tỉa & Spa (Grooming Staff) để phụ trách dịch vụ '{bookingItem.Service.Name}'.",
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

            // Tự động đồng bộ sang dịch vụ Diet đi kèm nếu đây là dịch vụ Boarding
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
            if (from == to) return true; // Giữ nguyên trạng thái (khi Lễ tân gán nhân viên, xếp phòng...)
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

            // Nếu toàn bộ dịch vụ là No-Show -> Đơn chuyển sang No_Show
            if (itemStatuses.All(s => s == BookingItemStatus.No_Show))
            {
                booking.Status = BookingStatus.No_Show;
                if (string.IsNullOrEmpty(booking.CancellationReason))
                {
                    booking.CancellationReason = "Khách không đến theo lịch hẹn (No-Show)";
                }
            }
            // Nếu toàn bộ dịch vụ bị hủy hoặc kết hợp hủy/no-show -> Đơn bị Hủy
            else if (itemStatuses.All(s => s == BookingItemStatus.Cancelled || s == BookingItemStatus.No_Show))
            {
                booking.Status = BookingStatus.Cancelled;
            }
            // Đơn luôn giữ trạng thái Confirmed (đang hoạt động) nếu đã Confirmed để chờ Lễ tân kiểm tra và bấm Checkout bàn giao pet
            else if (booking.Status == BookingStatus.Confirmed && itemStatuses.Any(s => s == BookingItemStatus.Confirmed || s == BookingItemStatus.Checked_In || s == BookingItemStatus.In_Progress || s == BookingItemStatus.Completed))
            {
                booking.Status = BookingStatus.Confirmed;
            }
        }
    }
}
