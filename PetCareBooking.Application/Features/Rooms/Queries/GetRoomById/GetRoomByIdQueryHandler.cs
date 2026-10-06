using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Room;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, ApiResponse<RoomDetailResponseDTO>>
    {
        private readonly IGenericRepository<Room> _roomRepository;

        public GetRoomByIdQueryHandler(IGenericRepository<Room> roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<ApiResponse<RoomDetailResponseDTO>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetQueryable()
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (room == null)
            {
                return new ApiResponse<RoomDetailResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room with ID {request.Id} not found.",
                    Result = null
                };
            }

            var result = new RoomDetailResponseDTO
            {
                Id = room.Id,
                RoomName = room.RoomName,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType.Name,
                RoomTypeDescription = room.RoomType.Description,
                Status = room.Status,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };

            return new ApiResponse<RoomDetailResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get room detail successfully.",
                Result = result
            };
        }
    }
}
