using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetCustomerBookings
{
    public class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerBookingsQueryHandler(
            IGenericRepository<Booking> bookingRepository,
            ICurrentUserService currentUserService)
        {
            _bookingRepository = bookingRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PagedResult<BookingListResponseDTO>>> Handle(GetCustomerBookingsQuery request, CancellationToken cancellationToken)
        {
            // Kiểm tra phân quyền: Customer chỉ được xem booking của chính mình, Admin/Staff xem được của bất kỳ ai
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || request.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<PagedResult<BookingListResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You do not have permission to view another customer's bookings.",
                        Result = null
                    };
                }
            }

            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            DateTime? serviceDayStart = request.ServiceDate?.Date;
            DateTime? serviceDayEnd = serviceDayStart.HasValue ? serviceDayStart.Value.AddDays(1) : null;

            // Get customer bookings with filtering
            var bookings = await _bookingRepository.FindAsync(b =>
                b.CustomerId == request.CustomerId &&
                (!request.Status.HasValue || b.Status == request.Status.Value) &&
                (!request.ServiceType.HasValue || b.BookingItems.Any(bi => bi.Service.ServiceType == request.ServiceType.Value)) &&
                (!serviceDayStart.HasValue || b.BookingItems.Any(bi => 
                    (bi.ScheduledStartAt >= serviceDayStart.Value && bi.ScheduledStartAt < serviceDayEnd!.Value) ||
                    (bi.ScheduledEndAt.HasValue && bi.ScheduledStartAt <= serviceDayStart.Value && bi.ScheduledEndAt.Value >= serviceDayStart.Value))),
                "BookingItems.Service", "Customer");

            // Calculate total count before pagination
            int totalCount = bookings.Count();

            // Apply pagination
            var paginatedBookings = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTO
            var bookingDTOs = paginatedBookings.Select(b =>
            {
                bool isBoarding = b.BookingItems.Any(bi => bi.Service != null && bi.Service.ServiceType == ServiceType.Boarding);
                var firstStart = b.BookingItems.OrderBy(bi => bi.ScheduledStartAt).Select(bi => (DateTime?)bi.ScheduledStartAt).FirstOrDefault();
                var lastEnd = b.BookingItems.OrderByDescending(bi => bi.ScheduledEndAt ?? bi.ScheduledStartAt).Select(bi => bi.ScheduledEndAt).FirstOrDefault();

                return new BookingListResponseDTO
                {
                    Id = b.Id,
                    CustomerId = b.CustomerId,
                    CustomerName = b.Customer?.FullName ?? "Unknown",
                    ItemsCount = b.BookingItems.Count,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    BookingType = isBoarding ? "Boarding" : "Grooming",
                    ScheduledStartAt = firstStart,
                    ScheduledEndAt = lastEnd,
                    ServiceNames = b.BookingItems.Where(bi => bi.Service != null).Select(bi => bi.Service.Name).Distinct().ToList(),
                    HasPhotos = b.BookingItems.Any(bi => !string.IsNullOrEmpty(bi.BeforeImageUrl) || !string.IsNullOrEmpty(bi.ResultImageUrl))
                };
            }).ToList();

            return new ApiResponse<PagedResult<BookingListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get customer bookings successfully.",
                Result = new PagedResult<BookingListResponseDTO>
                {
                    Items = bookingDTOs,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
    }
}