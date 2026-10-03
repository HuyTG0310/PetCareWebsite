using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

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

            // Get all bookings with filtering
            var bookings = await _bookingRepository.FindAsync(b =>
                (!request.Status.HasValue || b.Status == request.Status.Value) &&
                (!request.DateFrom.HasValue || b.CreatedAt >= request.DateFrom.Value) &&
                (!request.DateTo.HasValue || b.CreatedAt <= request.DateTo.Value),
                "BookingItems,Customer");

            // Calculate total count before pagination
            int totalCount = bookings.Count();

            // Apply pagination
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