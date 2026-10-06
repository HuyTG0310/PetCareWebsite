using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRoomCommandHandler(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<RoomType> roomTypeRepository,
            IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra phòng có tồn tại hay không
            var existingRoom = await _roomRepository.GetByIdAsync(request.Id);

            if (existingRoom == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            // 2. Kiểm tra Loại phòng mới có tồn tại hay không
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

            var trimmedName = request.RoomName.Trim();

            // 3. Kiểm tra trùng tên phòng với các phòng khác (trừ chính nó)
            var isDuplicateName = await _roomRepository.GetQueryable()
                .AnyAsync(r => r.Id != request.Id && r.RoomName.ToLower() == trimmedName.ToLower(), cancellationToken);

            if (isDuplicateName)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Room with name '{trimmedName}' already exists.",
                    Result = Guid.Empty
                };
            }

            // 4. Cập nhật thông tin
            existingRoom.RoomName = trimmedName;
            existingRoom.RoomTypeId = request.RoomTypeId;
            existingRoom.UpdatedAt = DateTime.UtcNow;

            _roomRepository.Update(existingRoom);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Update room successfully.",
                Result = existingRoom.Id
            };
        }
    }
}
