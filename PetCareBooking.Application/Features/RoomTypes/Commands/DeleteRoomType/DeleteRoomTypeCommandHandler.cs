using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.DeleteRoomType
{
    public class DeleteRoomTypeCommandHandler : IRequestHandler<DeleteRoomTypeCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRoomTypeCommandHandler(IGenericRepository<RoomType> roomTypeRepository, IUnitOfWork unitOfWork)
        {
            _roomTypeRepository = roomTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(DeleteRoomTypeCommand request, CancellationToken cancellationToken)
        {
            var roomType = await _roomTypeRepository.GetQueryable()
                .Include(rt => rt.Rooms)
                .FirstOrDefaultAsync(rt => rt.Id == request.Id, cancellationToken);

            if (roomType == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room type with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            // Kiểm tra ràng buộc: Không cho xóa nếu vẫn còn phòng thuộc loại phòng này
            if (roomType.Rooms != null && roomType.Rooms.Any())
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot delete room type '{roomType.Name}' because it currently contains {roomType.Rooms.Count} room(s). Please delete or reassign those rooms first.",
                    Result = roomType.Id
                };
            }

            _roomTypeRepository.Delete(roomType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Delete room type successfully.",
                Result = roomType.Id
            };
        }
    }
}
