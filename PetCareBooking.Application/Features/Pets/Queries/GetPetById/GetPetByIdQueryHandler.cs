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

        public GetPetByIdQueryHandler(IGenericRepository<Pet> petRepository)
        {
            _petRepository = petRepository;
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
