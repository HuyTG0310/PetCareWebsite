using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<BookingItem> _bookingItemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteRoomCommandHandler(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<BookingItem> bookingItemRepository,
            IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _bookingItemRepository = bookingItemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
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

            // Kiểm tra ràng buộc: Phòng đã từng có lịch đặt trong quá khứ hoặc tương lai hay chưa
            var hasBookingHistory = await _bookingItemRepository.GetQueryable()
                .AnyAsync(bi => bi.RoomId == request.Id, cancellationToken);

            if (hasBookingHistory)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Cannot delete room '{room.RoomName}' because it has associated booking history. To stop using this room, please update its status to 'Maintenance' instead.",
                    Result = room.Id
                };
            }

            _roomRepository.Delete(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Delete room '{room.RoomName}' successfully.",
                Result = room.Id
            };
        }
    }
}
