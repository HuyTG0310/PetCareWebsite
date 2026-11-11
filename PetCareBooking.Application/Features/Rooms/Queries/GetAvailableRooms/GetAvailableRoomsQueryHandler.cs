using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetAvailableRooms
{
    public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, ApiResponse<List<AvailableRoomResponseDTO>>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;

        public GetAvailableRoomsQueryHandler(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<BookingItem> bookingItemRepository)
        {
            _roomRepository = roomRepository;
            _bookingItemRepository = bookingItemRepository;
        }

        public async Task<ApiResponse<List<AvailableRoomResponseDTO>>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
        {
            // 1. Scan BookingItem table to find rooms occupied within [request.From, request.To]
            var occupiedRoomIds = await _bookingItemRepository.GetQueryable()
                .Where(bi => bi.RoomId.HasValue &&
                             bi.Status != BookingItemStatus.Cancelled &&
                             bi.Status != BookingItemStatus.No_Show &&
                             bi.ScheduledStartAt < request.To &&
                             (bi.ScheduledEndAt == null ? bi.ScheduledStartAt.AddDays(1) : bi.ScheduledEndAt.Value) > request.From)
                .Select(bi => bi.RoomId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 2. Query Room table to find rooms that are truly available:
            // - Physical status is not under maintenance (Status == Available)
            // - Not in the list of currently occupied rooms (occupiedRoomIds)
            // - Filter by RoomTypeId if specified by user
            var query = _roomRepository.GetQueryable()
                .Include(r => r.RoomType)
                .Where(r => r.Status == RoomStatus.Available && !occupiedRoomIds.Contains(r.Id));

            if (request.RoomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == request.RoomTypeId.Value);
            }

            var availableRooms = await query
                .OrderBy(r => r.RoomName)
                .Select(r => new AvailableRoomResponseDTO
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    RoomTypeId = r.RoomTypeId,
                    RoomTypeName = r.RoomType.Name,
                    Status = r.Status
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<List<AvailableRoomResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get available rooms successfully.",
                Result = availableRooms
            };
        }
    }
}
