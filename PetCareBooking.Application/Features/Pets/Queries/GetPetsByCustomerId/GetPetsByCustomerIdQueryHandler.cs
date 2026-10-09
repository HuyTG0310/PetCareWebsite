using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Pets.Queries.GetPetsByCustomerId
{
    public class GetPetsByCustomerIdQueryHandler : IRequestHandler<GetPetsByCustomerIdQuery, ApiResponse<List<PetResponseDTO>>>
    {
        private readonly IGenericRepository<Pet> _petRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPetsByCustomerIdQueryHandler(
            IGenericRepository<Pet> petRepository,
            IGenericRepository<Customer> customerRepository,
            ICurrentUserService currentUserService)
        {
            _petRepository = petRepository;
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<List<PetResponseDTO>>> Handle(GetPetsByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            // Kiểm tra quyền: Customer chỉ được xem pet của chính mình, Admin/Staff được xem của bất kỳ ai
            if (!_currentUserService.IsAdminOrStaff)
            {
                if (!_currentUserService.UserId.HasValue || request.CustomerId != _currentUserService.UserId.Value)
                {
                    return new ApiResponse<List<PetResponseDTO>>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "You do not have permission to view another customer's pets.",
                        Result = null
                    };
                }
            }

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<List<PetResponseDTO>>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Customer with ID {request.CustomerId} not found.",
                    Result = null
                };
            }

            var query = _petRepository.GetQueryable()
                .AsNoTracking()
                .Where(p => p.CustomerId == request.CustomerId);

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            var pets = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PetResponseDTO
                {
                    Id = p.Id,
                    CustomerId = p.CustomerId,
                    OwnerName = customer.FullName,
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
                .ToListAsync(cancellationToken);

            return new ApiResponse<List<PetResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Get customer's pets successfully.",
                Result = pets
            };
        }
    }
}

