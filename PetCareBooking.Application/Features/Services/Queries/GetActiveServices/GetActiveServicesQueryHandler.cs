using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Queries.GetActiveServices
{
    public class GetActiveServicesQueryHandler : IRequestHandler<GetActiveServicesQuery, ApiResponse<List<ServiceResponseDTO>>>
    {
        private readonly IGenericRepository<Service> _repository;

        public GetActiveServicesQueryHandler(IGenericRepository<Service> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ServiceResponseDTO>>> Handle(GetActiveServicesQuery request, CancellationToken cancellationToken)
        {
            var services = await _repository.FindAsync(x => x.IsActive == true, "ServicePrices");

            var serviceDTOs = services.Select(s => new ServiceResponseDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                ServiceType = s.ServiceType,
                IsActive = s.IsActive,
                Prices = s.ServicePrices.Select(p => new ServicePriceResponseDTO
                {
                    Id = p.Id,
                    MinWeight = p.MinWeight,
                    MaxWeight = p.MaxWeight,
                    Price = p.Price,
                    PricingUnit = p.PricingUnit
                }).ToList()
            }).ToList();

            return new ApiResponse<List<ServiceResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get active services successfully.",
                Result = serviceDTOs
            };
        }
    }
}
