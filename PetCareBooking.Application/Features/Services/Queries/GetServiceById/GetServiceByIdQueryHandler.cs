using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Queries.GetServiceById
{
    public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ApiResponse<ServiceResponseDTO>>
    {
        private readonly IGenericRepository<Service> _repository;

        public GetServiceByIdQueryHandler(IGenericRepository<Service> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ServiceResponseDTO>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        {
            // Sử dụng FindAsync để include bảng giá, sau đó lấy phần tử đầu tiên
            var services = await _repository.FindAsync(s => s.Id == request.Id, "ServicePrices");
            var service = services.FirstOrDefault();

            if (service == null)
            {
                return new ApiResponse<ServiceResponseDTO>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Service with ID {request.Id} not found.",
                    Result = null
                };
            }

            var serviceDTO = new ServiceResponseDTO
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ServiceType = service.ServiceType,
                IsActive = service.IsActive,
                Prices = service.ServicePrices.Select(p => new ServicePriceResponseDTO
                {
                    Id = p.Id,
                    MinWeight = p.MinWeight,
                    MaxWeight = p.MaxWeight,
                    Price = p.Price,
                    PricingUnit = p.PricingUnit
                }).ToList()
            };

            return new ApiResponse<ServiceResponseDTO>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get service details successfully.",
                Result = serviceDTO
            };
        }
    }
}
