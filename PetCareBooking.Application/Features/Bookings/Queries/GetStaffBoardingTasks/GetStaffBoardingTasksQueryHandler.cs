using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetStaffBoardingTasks
{
    public class GetStaffBoardingTasksQueryHandler : IRequestHandler<GetStaffBoardingTasksQuery, ApiResponse<PagedResult<BoardingTaskResponseDTO>>>
    {
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;

        public GetStaffBoardingTasksQueryHandler(IGenericRepository<BookingItem> bookingItemRepository)
        {
            _bookingItemRepository = bookingItemRepository;
        }

        public async Task<ApiResponse<PagedResult<BoardingTaskResponseDTO>>> Handle(GetStaffBoardingTasksQuery request, CancellationToken cancellationToken)
        {
            // 1. Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            // 2. Fetch Boarding items (Diet is attached as an add-on option to the Boarding card)
            var items = await _bookingItemRepository.FindAsync(
                bi => bi.Service.ServiceType == ServiceType.Boarding &&
                      (!request.StaffId.HasValue || bi.StaffId == request.StaffId.Value) &&
                      (!request.Status.HasValue || bi.Status == request.Status.Value) &&
                      (!request.OnlyActiveGuests || (bi.Status == BookingItemStatus.Checked_In || bi.Status == BookingItemStatus.In_Progress)) &&
                      (!request.Date.HasValue || (bi.ScheduledStartAt.Date <= request.Date.Value.Date &&
                                                 (!bi.ScheduledEndAt.HasValue || bi.ScheduledEndAt.Value.Date >= request.Date.Value.Date))),
                "Pet", "Service", "Room.RoomType", "Booking.Customer", "Booking.BookingItems.Service", "Staff", "CareRecords"
            );

            var itemsList = items.ToList();
            int totalCount = itemsList.Count;

            // 3. Paginate and sort by scheduled start time
            var paginatedItems = itemsList
                .OrderBy(bi => bi.ScheduledStartAt)
                .ThenBy(bi => bi.Room != null ? bi.Room.RoomName : string.Empty)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 4. Map to DTO
            var taskDTOs = paginatedItems.Select(bi =>
            {
                var latestRecord = bi.CareRecords?
                    .OrderByDescending(c => c.RecordDate)
                    .FirstOrDefault();

                var dietItem = bi.Booking?.BookingItems?.FirstOrDefault(other =>
                    other.PetId == bi.PetId &&
                    other.Service != null &&
                    other.Service.ServiceType == ServiceType.Diet);

                return new BoardingTaskResponseDTO
                {
                    BookingId = bi.BookingId,
                    ItemId = bi.Id,
                    CustomerId = bi.Booking != null ? bi.Booking.CustomerId : Guid.Empty,
                    CustomerName = bi.Booking?.Customer != null ? bi.Booking.Customer.FullName : string.Empty,
                    CustomerPhone = bi.Booking?.Customer != null ? bi.Booking.Customer.PhoneNumber : string.Empty,
                    CustomerEmail = bi.Booking?.Customer?.Email,
                    PetId = bi.PetId,
                    PetName = bi.Pet != null ? bi.Pet.Name : string.Empty,
                    PetSpecies = bi.Pet != null ? bi.Pet.Species : default,
                    PetBreed = bi.Pet?.Breed,
                    PetWeight = bi.Pet != null ? bi.Pet.Weight : 0,
                    PetHealthNotes = bi.Pet?.HealthNotes,
                    PetAvatarUrl = bi.Pet?.ImageUrl,
                    ServiceId = bi.ServiceId,
                    ServiceName = bi.Service != null ? bi.Service.Name : string.Empty,
                    ServiceType = bi.Service != null ? bi.Service.ServiceType : default,
                    DietServiceName = dietItem?.Service?.Name,
                    DietPrice = dietItem?.AssignedPrice,
                    RoomId = bi.RoomId,
                    RoomName = bi.Room?.RoomName,
                    RoomTypeName = bi.Room?.RoomType?.Name,
                    StaffId = bi.StaffId,
                    StaffName = bi.Staff?.FullName,
                    ScheduledStartAt = bi.ScheduledStartAt,
                    ScheduledEndAt = bi.ScheduledEndAt,
                    Quantity = bi.Quantity,
                    Status = bi.Status,
                    ResultNote = bi.ResultNote,
                    TotalCareRecords = bi.CareRecords?.Count ?? 0,
                    LatestCareRecordDate = latestRecord?.RecordDate,
                    LatestHealthStatus = latestRecord?.HealthStatus,
                    LatestNote = latestRecord?.Note,
                    LatestImageUrl = latestRecord?.ImageUrl
                };
            }).ToList();

            return new ApiResponse<PagedResult<BoardingTaskResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get staff boarding tasks successfully.",
                Result = new PagedResult<BoardingTaskResponseDTO>
                {
                    Items = taskDTOs,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
    }
}

