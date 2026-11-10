using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeAvailability
{
    public class GetRoomTypeAvailabilityQueryHandler : IRequestHandler<GetRoomTypeAvailabilityQuery, ApiResponse<List<RoomTypeAvailabilityResponseDTO>>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;

        public GetRoomTypeAvailabilityQueryHandler(
            IGenericRepository<RoomType> roomTypeRepository,
            IGenericRepository<BookingItem> bookingItemRepository)
        {
            _roomTypeRepository = roomTypeRepository;
            _bookingItemRepository = bookingItemRepository;
        }

        public async Task<ApiResponse<List<RoomTypeAvailabilityResponseDTO>>> Handle(GetRoomTypeAvailabilityQuery request, CancellationToken cancellationToken)
        {
            // 1. Find rooms (RoomId) occupied within [request.From, request.To]
            // Overlap condition between two date intervals:
            // Booking.Start < Request.To && Booking.End > Request.From
            var occupiedRoomIds = await _bookingItemRepository.GetQueryable()
                .Where(bi => bi.RoomId.HasValue &&
                             bi.Status != BookingItemStatus.Cancelled &&
                             bi.Status != BookingItemStatus.No_Show &&
                             bi.ScheduledStartAt < request.To &&
                             (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > request.From)
                .Select(bi => bi.RoomId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 2. Retrieve Room Types along with physical rooms under each type
            var roomTypes = await _roomTypeRepository.GetQueryable()
                .Include(rt => rt.Rooms)
                .OrderBy(rt => rt.Name)
                .ToListAsync(cancellationToken);

            // 3. Calculate available room count for each room type
            // Available room criteria:
            // - Physical status is not under maintenance (Status == Available)
            // - Not in the list of overlapping occupied rooms (occupiedRoomIds)
            var response = roomTypes.Select(rt =>
            {
                var totalRooms = rt.Rooms.Count;
                var availableRooms = rt.Rooms.Count(r =>
                    r.Status == RoomStatus.Available &&
                    !occupiedRoomIds.Contains(r.Id));

                return new RoomTypeAvailabilityResponseDTO
                {
                    RoomTypeId = rt.Id,
                    RoomTypeName = rt.Name,
                    Description = rt.Description,
                    ImageUrl = rt.ImageUrl,
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms
                };
            }).ToList();

            return new ApiResponse<List<RoomTypeAvailabilityResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Check room type availability successfully.",
                Result = response
            };
        }
    }
}
