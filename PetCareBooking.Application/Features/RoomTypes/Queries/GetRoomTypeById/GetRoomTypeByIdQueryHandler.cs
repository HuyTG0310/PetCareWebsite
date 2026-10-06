using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.RoomType;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.RoomTypes.Queries.GetRoomTypeById
{
    public class GetRoomTypeByIdQueryHandler : IRequestHandler<GetRoomTypeByIdQuery, ApiResponse<RoomTypeDetailResponseDTO>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;

        public GetRoomTypeByIdQueryHandler(IGenericRepository<RoomType> roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<ApiResponse<RoomTypeDetailResponseDTO>> Handle(GetRoomTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var roomType = await _roomTypeRepository.GetQueryable()
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.Id == request.Id, cancellationToken);

            if (roomType == null)
            {
                return new ApiResponse<RoomTypeDetailResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room type with ID {request.Id} not found.",
                    Result = null
                };
            }

            var result = new RoomTypeDetailResponseDTO
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                TotalRooms = roomType.Rooms.Count,
                Rooms = roomType.Rooms.Select(r => new RoomSummaryDTO
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    Status = r.Status
                }).OrderBy(r => r.RoomName).ToList()
            };

            return new ApiResponse<RoomTypeDetailResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get room type detail successfully.",
                Result = result
            };
        }
    }
}
