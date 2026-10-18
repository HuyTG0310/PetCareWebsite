using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.SearchBookings
{
    public class SearchBookingsQueryHandler : IRequestHandler<SearchBookingsQuery, ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;

        public SearchBookingsQueryHandler(IGenericRepository<Booking> bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse<PagedResult<BookingListResponseDTO>>> Handle(SearchBookingsQuery request, CancellationToken cancellationToken)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            DateTime? serviceDayStart = request.ServiceDate?.Date;
            DateTime? serviceDayEnd = serviceDayStart.HasValue ? serviceDayStart.Value.AddDays(1) : null;

            // Build search query with all filters
            var bookings = await _bookingRepository.FindAsync(b =>
                // 1. Filter by keyword (search in customer name, email, phone)
                (string.IsNullOrEmpty(request.Keyword) ||
                 b.Customer.FullName.ToLower().Contains(request.Keyword.ToLower()) ||
                 b.Customer.Email.ToLower().Contains(request.Keyword.ToLower()) ||
                 b.Customer.PhoneNumber.Contains(request.Keyword)) &&

                // 2. Filter by booking status
                (!request.Status.HasValue || b.Status == request.Status.Value) &&

                // 3. Filter by date range (CreatedAt)
                (!request.DateFrom.HasValue || b.CreatedAt >= request.DateFrom.Value) &&
                (!request.DateTo.HasValue || b.CreatedAt <= request.DateTo.Value) &&

                // 4. Filter by service appointment date
                (!serviceDayStart.HasValue || b.BookingItems.Any(bi => bi.ScheduledStartAt >= serviceDayStart.Value && bi.ScheduledStartAt < serviceDayEnd!.Value)),

                // Include related data
                "BookingItems.Service", "Customer");

            // Calculate total count before pagination
            int totalCount = bookings.Count();

            // Apply pagination and ordering (newest first)
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

                var activeItems = b.BookingItems
                    .Where(bi => bi.Status != BookingItemStatus.Cancelled && bi.Status != BookingItemStatus.No_Show)
                    .ToList();

                bool isFullyAssigned = activeItems.All(bi =>
                {
                    bool isBoarding = bi.Service != null && bi.Service.ServiceType == ServiceType.Boarding;
                    bool isDiet = bi.Service != null && bi.Service.ServiceType == ServiceType.Diet;
                    if (isDiet) return true;
                    if (isBoarding) return bi.StaffId.HasValue && bi.RoomId.HasValue;
                    return bi.StaffId.HasValue;
                });

                bool canCheckIn = b.Status == BookingStatus.Confirmed &&
                                  isFullyAssigned &&
                                  activeItems.Any(bi => bi.Status == BookingItemStatus.Pending || bi.Status == BookingItemStatus.Confirmed);

                bool hasInServiceItems = activeItems.Any(bi => bi.Status == BookingItemStatus.Checked_In || bi.Status == BookingItemStatus.In_Progress);

                bool canCheckout = b.Status == BookingStatus.Confirmed &&
                                   activeItems.Any() &&
                                   activeItems.All(bi => bi.Status == BookingItemStatus.Completed);

                bool canMarkNoShow = (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                                     !b.BookingItems.Any(bi => bi.Status == BookingItemStatus.Checked_In || bi.Status == BookingItemStatus.In_Progress || bi.Status == BookingItemStatus.Completed);

                bool isNoShow = b.Status == BookingStatus.No_Show ||
                                (b.Status == BookingStatus.Cancelled && (b.CancellationReason != null && b.CancellationReason.Contains("No-Show", StringComparison.OrdinalIgnoreCase))) ||
                                (b.BookingItems.Any() && b.BookingItems.All(bi => bi.Status == BookingItemStatus.No_Show));

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
                    CanCheckIn = canCheckIn,
                    CanCheckout = canCheckout,
                    HasInServiceItems = hasInServiceItems,
                    IsFullyAssigned = isFullyAssigned,
                    CancellationReason = b.CancellationReason,
                    CanMarkNoShow = canMarkNoShow,
                    IsNoShow = isNoShow
                };
            }).ToList();

            return new ApiResponse<PagedResult<BookingListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Search bookings successfully.",
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