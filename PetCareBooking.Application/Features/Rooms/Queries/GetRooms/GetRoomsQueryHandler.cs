using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetRooms
{
    public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, ApiResponse<PagedResult<RoomResponseDTO>>>
    {
        private readonly IGenericRepository<Room> _roomRepository;

        public GetRoomsQueryHandler(IGenericRepository<Room> roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<ApiResponse<PagedResult<RoomResponseDTO>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            var query = _roomRepository.GetQueryable()
                .Include(r => r.RoomType)
                .AsQueryable();

            // 1. Lọc theo Loại phòng nếu có
            if (request.RoomTypeId.HasValue)
            {
                query = query.Where(r => r.RoomTypeId == request.RoomTypeId.Value);
            }

            // 2. Lọc theo Trạng thái nếu có
            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            // 3. Tìm kiếm theo tên phòng hoặc tên loại phòng
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(r => r.RoomName.ToLower().Contains(term) ||
                                         r.RoomType.Name.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(r => r.RoomName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new RoomResponseDTO
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    RoomTypeId = r.RoomTypeId,
                    RoomTypeName = r.RoomType.Name,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new ApiResponse<PagedResult<RoomResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get rooms successfully.",
                Result = new PagedResult<RoomResponseDTO>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
    }
}
