using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.CareRecord;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.CareRecords.Queries.GetCareRecordsByBookingItem
{
    public class GetCareRecordsByBookingItemQueryHandler : IRequestHandler<GetCareRecordsByBookingItemQuery, ApiResponse<List<CareRecordResponseDTO>>>
    {
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IGenericRepository<CareRecord> _careRecordRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCareRecordsByBookingItemQueryHandler(
            IGenericRepository<BookingItem> bookingItemRepository,
            IGenericRepository<CareRecord> careRecordRepository,
            ICurrentUserService currentUserService)
        {
            _bookingItemRepository = bookingItemRepository;
            _careRecordRepository = careRecordRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<List<CareRecordResponseDTO>>> Handle(GetCareRecordsByBookingItemQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch BookingItem with parent booking to check authorization
            var bookingItems = await _bookingItemRepository.FindAsync(
                bi => bi.Id == request.BookingItemId,
                "Booking", "Pet"
            );

            var bookingItem = bookingItems.FirstOrDefault();
            if (bookingItem == null)
            {
                return new ApiResponse<List<CareRecordResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Booking item with ID {request.BookingItemId} not found.",
                    Result = null
                };
            }

            // 2. Chống IDOR: Khách hàng chỉ được xem nhật ký chăm sóc của thú cưng thuộc đơn hàng của chính mình
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || bookingItem.Booking.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<List<CareRecordResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to view care records of this booking item.",
                        Result = null
                    };
                }
            }

            // 3. Fetch Care Records for this booking item
            var records = await _careRecordRepository.FindAsync(
                cr => cr.BookingItemId == request.BookingItemId,
                "Staff", "BookingItem.Pet"
            );

            var result = records
                .OrderByDescending(cr => cr.RecordDate)
                .Select(cr => new CareRecordResponseDTO
                {
                    Id = cr.Id,
                    BookingItemId = cr.BookingItemId,
                    StaffId = cr.StaffId,
                    StaffName = cr.Staff != null ? cr.Staff.FullName : null,
                    PetName = cr.BookingItem != null && cr.BookingItem.Pet != null ? cr.BookingItem.Pet.Name : bookingItem.Pet?.Name,
                    RecordDate = cr.RecordDate,
                    HealthStatus = cr.HealthStatus,
                    Note = cr.Note,
                    ImageUrl = cr.ImageUrl,
                    CreatedAt = cr.CreatedAt,
                    UpdatedAt = cr.UpdatedAt
                })
                .ToList();

            return new ApiResponse<List<CareRecordResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Get care records by booking item successfully.",
                Result = result
            };
        }
    }
}

