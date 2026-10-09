using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Queries.GetPetById
{
    public class GetPetByIdQueryHandler : IRequestHandler<GetPetByIdQuery, ApiResponse<PetResponseDTO>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPetByIdQueryHandler(
            IGenericRepository<Pet> petRepository,
            ICurrentUserService currentUserService)
        {
            _petRepository = petRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PetResponseDTO>> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
        {
            var pet = await _petRepository.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Id == request.Id)
                .Select(p => new PetResponseDTO
                {
                    Id = p.Id,
                    CustomerId = p.CustomerId,
                    OwnerName = p.Owner != null ? p.Owner.FullName : null,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    Weight = p.Weight,
                    Age = p.Age,
                    HealthNotes = p.HealthNotes,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (pet == null)
            {
                return new ApiResponse<PetResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Pet with ID {request.Id} not found.",
                    Result = null
                };
            }

            // Kiểm tra quyền xem: Nếu không phải Admin/Staff thì chỉ được xem pet của chính mình
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || pet.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<PetResponseDTO>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to view this pet.",
                        Result = null
                    };
                }
            }

            return new ApiResponse<PetResponseDTO>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Get pet detail successfully.",
                Result = pet
            };
        }
    }
}
