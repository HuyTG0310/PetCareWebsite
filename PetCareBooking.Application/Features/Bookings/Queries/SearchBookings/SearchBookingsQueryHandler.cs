using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

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
                (!request.DateTo.HasValue || b.CreatedAt <= request.DateTo.Value),

                // Include related data
                "BookingItems,Customer");

            // Calculate total count before pagination
            int totalCount = bookings.Count();

            // Apply pagination and ordering (newest first)
            var paginatedBookings = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTO
            var bookingDTOs = paginatedBookings.Select(b => new BookingListResponseDTO
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer.FullName,
                ItemsCount = b.BookingItems.Count,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                CreatedAt = b.CreatedAt
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