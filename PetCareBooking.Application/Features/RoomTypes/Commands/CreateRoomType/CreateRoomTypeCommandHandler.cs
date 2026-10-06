using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.CreateRoomType
{
    public class CreateRoomTypeCommandHandler : IRequestHandler<CreateRoomTypeCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoomTypeCommandHandler(IGenericRepository<RoomType> roomTypeRepository, IUnitOfWork unitOfWork)
        {
            _roomTypeRepository = roomTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateRoomTypeCommand request, CancellationToken cancellationToken)
        {
            var trimmedName = request.Name.Trim();

            // Kiểm tra trùng tên loại phòng (không phân biệt hoa thường)
            var isDuplicate = await _roomTypeRepository.GetQueryable()
                .AnyAsync(rt => rt.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

            if (isDuplicate)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Room type with name '{trimmedName}' already exists.",
                    Result = Guid.Empty
                };
            }

            var roomType = new RoomType
            {
                Id = Guid.NewGuid(),
                Name = trimmedName,
                Description = request.Description?.Trim()
            };

            await _roomTypeRepository.AddAsync(roomType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Create room type successfully.",
                Result = roomType.Id
            };
        }
    }
}
