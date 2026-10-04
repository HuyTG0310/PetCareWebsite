using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Booking;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Bookings.Queries.GetStaffGroomingTasks
{
    public class GetStaffGroomingTasksQueryHandler : IRequestHandler<GetStaffGroomingTasksQuery, ApiResponse<PagedResult<GroomingTaskResponseDTO>>>
    {
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;

        public GetStaffGroomingTasksQueryHandler(IGenericRepository<BookingItem> bookingItemRepository)
        {
            _bookingItemRepository = bookingItemRepository;
        }

        public async Task<ApiResponse<PagedResult<GroomingTaskResponseDTO>>> Handle(GetStaffGroomingTasksQuery request, CancellationToken cancellationToken)
        {
            // 1. Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            // 2. Fetch Grooming tasks assigned to this staff
            var items = await _bookingItemRepository.FindAsync(
                bi => bi.StaffId == request.StaffId &&
                      bi.Service.ServiceType == ServiceType.Grooming &&
                      (!request.Status.HasValue || bi.Status == request.Status.Value) &&
                      (!request.Date.HasValue || bi.ScheduledStartAt.Date == request.Date.Value.Date),
                "Pet", "Service", "Room", "Booking.Customer"
            );

            var itemsList = items.ToList();
            int totalCount = itemsList.Count;

            // 3. Paginate and sort by scheduled start time
            var paginatedItems = itemsList
                .OrderBy(bi => bi.ScheduledStartAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 4. Map to DTO
            var taskDTOs = paginatedItems.Select(bi => new GroomingTaskResponseDTO
            {
                BookingId = bi.BookingId,
                ItemId = bi.Id,
                CustomerId = bi.Booking.CustomerId,
                CustomerName = bi.Booking.Customer != null ? bi.Booking.Customer.FullName : string.Empty,
                CustomerPhone = bi.Booking.Customer != null ? bi.Booking.Customer.PhoneNumber : string.Empty,
                PetId = bi.PetId,
                PetName = bi.Pet != null ? bi.Pet.Name : string.Empty,
                PetSpecies = bi.Pet != null ? bi.Pet.Species : default,
                PetBreed = bi.Pet?.Breed,
                PetWeight = bi.Pet != null ? bi.Pet.Weight : 0,
                PetHealthNotes = bi.Pet?.HealthNotes,
                ServiceId = bi.ServiceId,
                ServiceName = bi.Service != null ? bi.Service.Name : string.Empty,
                ServiceType = bi.Service != null ? bi.Service.ServiceType : default,
                RoomId = bi.RoomId,
                RoomName = bi.Room?.RoomName,
                ScheduledStartAt = bi.ScheduledStartAt,
                ScheduledEndAt = bi.ScheduledEndAt,
                Quantity = bi.Quantity,
                Status = bi.Status,
                ResultNote = bi.ResultNote,
                ResultImageUrl = bi.ResultImageUrl
            }).ToList();

            return new ApiResponse<PagedResult<GroomingTaskResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get staff grooming tasks successfully.",
                Result = new PagedResult<GroomingTaskResponseDTO>
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
