using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Rooms.Queries.CheckRoomAvailability
{
    public class CheckRoomAvailabilityQueryHandler : IRequestHandler<CheckRoomAvailabilityQuery, ApiResponse<RoomAvailabilityCalendarResponseDTO>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IGenericRepository<Service> _serviceRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;

        public CheckRoomAvailabilityQueryHandler(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<RoomType> roomTypeRepository,
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<BookingItem> bookingItemRepository)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
            _serviceRepository = serviceRepository;
            _bookingItemRepository = bookingItemRepository;
        }

        public async Task<ApiResponse<RoomAvailabilityCalendarResponseDTO>> Handle(CheckRoomAvailabilityQuery request, CancellationToken cancellationToken)
        {
            // 1. Xác định RoomTypeId từ ServiceId hoặc RoomTypeId
            Guid targetRoomTypeId;
            string? serviceName = null;
            string roomTypeName;

            if (request.ServiceId.HasValue)
            {
                var services = await _serviceRepository.FindAsync(s => s.Id == request.ServiceId.Value, "RoomType");
                var service = services.FirstOrDefault();

                if (service == null)
                {
                    return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Service with ID {request.ServiceId.Value} not found.",
                        Result = null
                    };
                }

                if (!service.RoomTypeId.HasValue)
                {
                    return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Service '{service.Name}' is not linked to any Room Type.",
                        Result = null
                    };
                }

                targetRoomTypeId = service.RoomTypeId.Value;
                serviceName = service.Name;
                roomTypeName = service.RoomType?.Name ?? string.Empty;
            }
            else if (request.RoomTypeId.HasValue)
            {
                targetRoomTypeId = request.RoomTypeId.Value;
                var roomType = await _roomTypeRepository.GetByIdAsync(targetRoomTypeId);

                if (roomType == null)
                {
                    return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"Room type with ID {targetRoomTypeId} not found.",
                        Result = null
                    };
                }

                roomTypeName = roomType.Name;
            }
            else
            {
                return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Either ServiceId or RoomTypeId must be provided.",
                    Result = null
                };
            }

            // 2. Xác định khoảng thời gian cần kiểm tra [fromDate, toDate]
            DateTime fromDate;
            DateTime toDate;

            if (request.Month.HasValue && request.Year.HasValue)
            {
                if (request.Month.Value < 1 || request.Month.Value > 12)
                {
                    return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Month must be between 1 and 12.",
                        Result = null
                    };
                }

                int daysInMonth = DateTime.DaysInMonth(request.Year.Value, request.Month.Value);
                fromDate = new DateTime(request.Year.Value, request.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                toDate = new DateTime(request.Year.Value, request.Month.Value, daysInMonth, 23, 59, 59, DateTimeKind.Utc);
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                fromDate = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
                toDate = DateTime.SpecifyKind(request.EndDate.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1);

                if (toDate < fromDate)
                {
                    return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "EndDate must be greater than or equal to StartDate.",
                        Result = null
                    };
                }
            }
            else if (request.StartDate.HasValue)
            {
                fromDate = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
                toDate = fromDate.AddDays(30).AddDays(1).AddTicks(-1);
            }
            else
            {
                var now = DateTime.UtcNow;
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                fromDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                toDate = new DateTime(now.Year, now.Month, daysInMonth, 23, 59, 59, DateTimeKind.Utc);
            }

            // 3. Lấy danh sách các phòng vật lý thuộc RoomType này (trạng thái Available, không bảo trì)
            var rooms = await _roomRepository.FindAsync(r => r.RoomTypeId == targetRoomTypeId && r.Status == RoomStatus.Available);
            var roomList = rooms.ToList();
            int totalRooms = roomList.Count;
            var roomIds = roomList.Select(r => r.Id).ToList();

            // 4. Lấy tất cả các BookingItem đang sử dụng các phòng này trong khoảng thời gian [fromDate, toDate]
            var occupiedBookings = await _bookingItemRepository.GetQueryable()
                .Where(bi => bi.RoomId.HasValue &&
                             roomIds.Contains(bi.RoomId.Value) &&
                             bi.Status != BookingItemStatus.Cancelled &&
                             bi.ScheduledStartAt < toDate &&
                             (bi.ScheduledEndAt ?? bi.ScheduledStartAt.AddDays(1)) > fromDate)
                .Select(bi => new
                {
                    RoomId = bi.RoomId!.Value,
                    bi.ScheduledStartAt,
                    ScheduledEndAt = bi.ScheduledEndAt ?? bi.ScheduledStartAt.AddDays(1)
                })
                .ToListAsync(cancellationToken);

            // 5. Tính toán số phòng còn trống cho từng ngày
            var dailyAvailabilities = new List<DailyRoomAvailabilityDTO>();
            for (var day = fromDate.Date; day <= toDate.Date; day = day.AddDays(1))
            {
                var dayStart = day;
                var dayEnd = day.AddDays(1);

                var occupiedCount = occupiedBookings
                    .Where(b => b.ScheduledStartAt < dayEnd && b.ScheduledEndAt > dayStart)
                    .Select(b => b.RoomId)
                    .Distinct()
                    .Count();

                int availableRooms = Math.Max(0, totalRooms - occupiedCount);

                dailyAvailabilities.Add(new DailyRoomAvailabilityDTO
                {
                    Date = day,
                    TotalRooms = totalRooms,
                    AvailableRooms = availableRooms
                });
            }

            // 6. Kiểm tra xem toàn bộ dải ngày có trống hay không
            bool? isFullRangeAvailable = null;
            int? minAvailableRoomsInRange = null;

            if (request.StartDate.HasValue && request.EndDate.HasValue && dailyAvailabilities.Count > 0)
            {
                isFullRangeAvailable = dailyAvailabilities.All(d => d.IsAvailable);
                minAvailableRoomsInRange = dailyAvailabilities.Min(d => d.AvailableRooms);
            }

            return new ApiResponse<RoomAvailabilityCalendarResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Check room availability successfully.",
                Result = new RoomAvailabilityCalendarResponseDTO
                {
                    ServiceId = request.ServiceId,
                    ServiceName = serviceName,
                    RoomTypeId = targetRoomTypeId,
                    RoomTypeName = roomTypeName,
                    TotalRooms = totalRooms,
                    FromDate = fromDate,
                    ToDate = toDate,
                    IsFullRangeAvailable = isFullRangeAvailable,
                    MinAvailableRoomsInRange = minAvailableRoomsInRange,
                    DailyAvailabilities = dailyAvailabilities
                }
            };
        }
    }
}

