using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Rooms.Commands.UpdateRoomStatus
{
    public class UpdateRoomStatusCommandHandler : IRequestHandler<UpdateRoomStatusCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRoomStatusCommandHandler(IGenericRepository<Room> roomRepository, IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateRoomStatusCommand request, CancellationToken cancellationToken)
        {
            var room = await _roomRepository.GetByIdAsync(request.Id);

            if (room == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            // Nếu trạng thái mới trùng với trạng thái hiện tại thì không cần cập nhật
            if (room.Status == request.Status)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = $"Room is already in '{request.Status}' status.",
                    Result = room.Id
                };
            }

            room.Status = request.Status;
            room.UpdatedAt = DateTime.UtcNow;

            _roomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Updated room '{room.RoomName}' status to '{room.Status}' successfully.",
                Result = room.Id
            };
        }
    }
}
