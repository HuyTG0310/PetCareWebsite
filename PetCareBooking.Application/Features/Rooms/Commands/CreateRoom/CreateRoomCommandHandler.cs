using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoomCommandHandler(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<RoomType> roomTypeRepository,
            IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra loại phòng có tồn tại hay không
            var roomTypeExists = await _roomTypeRepository.GetQueryable()
                .AnyAsync(rt => rt.Id == request.RoomTypeId, cancellationToken);

            if (!roomTypeExists)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room type with ID {request.RoomTypeId} does not exist.",
                    Result = Guid.Empty
                };
            }

            var trimmedRoomName = request.RoomName.Trim();

            // 2. Kiểm tra trùng tên phòng (không phân biệt hoa thường)
            var isDuplicateName = await _roomRepository.GetQueryable()
                .AnyAsync(r => r.RoomName.ToLower() == trimmedRoomName.ToLower(), cancellationToken);

            if (isDuplicateName)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Room with name '{trimmedRoomName}' already exists.",
                    Result = Guid.Empty
                };
            }

            // 3. Tạo phòng mới
            var newRoom = new Room
            {
                Id = Guid.NewGuid(),
                RoomTypeId = request.RoomTypeId,
                RoomName = trimmedRoomName,
                Status = request.Status ?? RoomStatus.Available
            };

            await _roomRepository.AddAsync(newRoom);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Create room successfully.",
                Result = newRoom.Id
            };
        }
    }
}
