using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Commands.DeletePet
{
    public class DeletePetCommandHandler : IRequestHandler<DeletePetCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePetCommandHandler(
            IGenericRepository<Pet> petRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeletePetCommand request, CancellationToken cancellationToken)
        {
            var pet = await _petRepository.GetByIdAsync(request.Id);
            if (pet == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Pet with ID {request.Id} not found.",
                    Result = false
                };
            }

            // Kiểm tra quyền sở hữu (Customer chỉ được xóa pet của mình)
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || pet.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to delete this pet.",
                        Result = false
                    };
                }
            }

            if (!pet.IsActive)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "This pet is already deleted (inactive).",
                    Result = false
                };
            }

            // Soft delete: deactivating the pet
            pet.IsActive = false;
            pet.UpdatedAt = DateTime.UtcNow;

            _petRepository.Update(pet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Pet deleted successfully.",
                Result = true
            };
        }
    }
}
