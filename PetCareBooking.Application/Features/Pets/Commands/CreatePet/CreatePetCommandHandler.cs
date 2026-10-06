using MediatR;
using Microsoft.AspNetCore.Http;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Commands.CreatePet
{
    public class CreatePetCommandHandler : IRequestHandler<CreatePetCommand, ApiResponse<Guid>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePetCommandHandler(
            IGenericRepository<Pet> petRepository,
            IGenericRepository<Customer> customerRepository,
            IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Guid>> Handle(CreatePetCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Customer not found.",
                    Result = Guid.Empty
                };
            }

            var pet = new Pet
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Name = request.Name.Trim(),
                Species = request.Species,
                Breed = request.Breed?.Trim(),
                Weight = request.Weight,
                Age = request.Age,
                HealthNotes = request.HealthNotes?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddAsync(pet);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "Pet created successfully.",
                Result = pet.Id
            };
        }
    }
}
