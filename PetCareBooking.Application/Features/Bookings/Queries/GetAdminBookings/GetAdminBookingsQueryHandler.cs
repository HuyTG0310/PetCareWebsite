using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetAdminBookings
{
    public class GetAdminBookingsQueryHandler : IRequestHandler<GetAdminBookingsQuery, ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;

        public GetAdminBookingsQueryHandler(IGenericRepository<Booking> bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse<PagedResult<BookingListResponseDTO>>> Handle(GetAdminBookingsQuery request, CancellationToken cancellationToken)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            DateTime? serviceDayStart = request.ServiceDate?.Date;
            DateTime? serviceDayEnd = serviceDayStart.HasValue ? serviceDayStart.Value.AddDays(1) : null;

            // Get all bookings with filtering
            var bookings = await _bookingRepository.FindAsync(b =>
                (!request.Status.HasValue || b.Status == request.Status.Value) &&
                (!request.DateFrom.HasValue || b.CreatedAt >= request.DateFrom.Value) &&
                (!request.DateTo.HasValue || b.CreatedAt <= request.DateTo.Value) &&
                (!serviceDayStart.HasValue || b.BookingItems.Any(bi => bi.ScheduledStartAt >= serviceDayStart.Value && bi.ScheduledStartAt < serviceDayEnd!.Value)),
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
                    ServiceNames = b.BookingItems.Where(bi => bi.Service != null).Select(bi => bi.Service.Name).Distinct().ToList()
                };
            }).ToList();

            return new ApiResponse<PagedResult<BookingListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get all bookings successfully.",
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