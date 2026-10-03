using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Bookings.Queries.SearchCustomerBookings
{
    public class SearchCustomerBookingsQueryHandler : IRequestHandler<SearchCustomerBookingsQuery, ApiResponse<PagedResult<BookingListResponseDTO>>>
    {
        private readonly IGenericRepository<Booking> _bookingRepository;

        public SearchCustomerBookingsQueryHandler(IGenericRepository<Booking> bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse<PagedResult<BookingListResponseDTO>>> Handle(SearchCustomerBookingsQuery request, CancellationToken cancellationToken)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            // Search customer's bookings only (LIMITED TO CUSTOMER)
            var bookings = await _bookingRepository.FindAsync(b =>
                // 1. CRITICAL: Only return bookings for this specific customer
                b.CustomerId == request.CustomerId &&

                // 2. Filter by booking status
                (!request.Status.HasValue || b.Status == request.Status.Value) &&

                // 3. Filter by keyword (search in service names within booking items)
                (string.IsNullOrEmpty(request.Keyword) ||
                 b.BookingItems.Any(bi => 
                    bi.Service.Name.ToLower().Contains(request.Keyword.ToLower()) ||
                    (bi.Pet.Name.ToLower().Contains(request.Keyword.ToLower())))),

                // Include related data
                "BookingItems.Service,BookingItems.Pet,Customer");

            // Calculate total count before pagination
            int totalCount = bookings.Count();

            // Apply pagination (newest first)
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
                Message = "Search customer bookings successfully.",
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