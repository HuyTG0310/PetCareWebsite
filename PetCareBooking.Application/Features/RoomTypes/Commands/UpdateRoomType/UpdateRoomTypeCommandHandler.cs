using MediatR;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.RoomTypes.Commands.UpdateRoomType
{
    public class UpdateRoomTypeCommandHandler : IRequestHandler<UpdateRoomTypeCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<RoomType> _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRoomTypeCommandHandler(IGenericRepository<RoomType> roomTypeRepository, IUnitOfWork unitOfWork)
        {
            _roomTypeRepository = roomTypeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateRoomTypeCommand request, CancellationToken cancellationToken)
        {
            var existingRoomType = await _roomTypeRepository.GetByIdAsync(request.Id);

            if (existingRoomType == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Room type with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            var trimmedName = request.Name.Trim();

            // Check for duplicate name with other room types (excluding itself)
            var isDuplicateName = await _roomTypeRepository.GetQueryable()
                .AnyAsync(rt => rt.Id != request.Id && rt.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

            if (isDuplicateName)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = $"Room type with name '{trimmedName}' already exists.",
                    Result = Guid.Empty
                };
            }

            existingRoomType.Name = trimmedName;
            existingRoomType.Description = request.Description?.Trim();

            _roomTypeRepository.Update(existingRoomType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Update room type successfully.",
                Result = existingRoomType.Id
            };
        }
    }
}
