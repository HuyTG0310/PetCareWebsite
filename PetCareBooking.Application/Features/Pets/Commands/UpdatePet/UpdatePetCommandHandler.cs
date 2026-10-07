using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Commands.UpdatePet
{
    public class UpdatePetCommandHandler : IRequestHandler<UpdatePetCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePetCommandHandler(
            IGenericRepository<Pet> petRepository,
            IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
        {
            var pet = await _petRepository.GetByIdAsync(request.Id);
            if (pet == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Pet with ID {request.Id} not found.",
                    Result = Guid.Empty
                };
            }

            pet.Name = request.Name.Trim();
            pet.Species = request.Species;
            pet.Breed = request.Breed?.Trim();
            pet.Weight = request.Weight;
            pet.Age = request.Age;
            pet.HealthNotes = request.HealthNotes?.Trim();
            pet.UpdatedAt = DateTime.UtcNow;

            _petRepository.Update(pet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Pet updated successfully.",
                Result = pet.Id
            };
        }
    }
}
