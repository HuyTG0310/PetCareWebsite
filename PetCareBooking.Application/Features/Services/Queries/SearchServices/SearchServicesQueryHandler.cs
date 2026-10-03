using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Queries.SearchServices
{
    public class SearchServicesQueryHandler : IRequestHandler<SearchServicesQuery, ApiResponse<PagedResult<ServiceListResponseDTO>>>
    {
        private readonly IGenericRepository<Service> _repository;

        public SearchServicesQueryHandler(IGenericRepository<Service> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<ServiceListResponseDTO>>> Handle(SearchServicesQuery request, CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            var services = await _repository.FindAsync(s =>
                s.IsActive == true &&

                (string.IsNullOrEmpty(request.Keyword) ||
                 s.Name.ToLower().Contains(request.Keyword.ToLower()) ||
                 (s.Description != null && s.Description.ToLower().Contains(request.Keyword.ToLower()))) &&

                (!request.ServiceType.HasValue || s.ServiceType == request.ServiceType.Value),

                "ServicePrices"
            );

            int totalCount = services.Count();

            var paginatedServices = services
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var serviceDTOs = paginatedServices.Select(s => new ServiceListResponseDTO
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                ServiceType = s.ServiceType,
                IsActive = s.IsActive
            }).ToList();

            return new ApiResponse<PagedResult<ServiceListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Search active services successfully.",
                Result = new PagedResult<ServiceListResponseDTO>
                {
                    Items = serviceDTOs,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
    }
}
