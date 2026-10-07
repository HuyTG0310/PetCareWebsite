using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Queries.SearchServicesForAdmin
{
    public class SearchServicesForAdminQueryHandler : IRequestHandler<SearchServicesForAdminQuery, ApiResponse<PagedResult<ServiceListResponseDTO>>>
    {
        private readonly IGenericRepository<Service> _repository;

        public SearchServicesForAdminQueryHandler(IGenericRepository<Service> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<ServiceListResponseDTO>>> Handle(SearchServicesForAdminQuery request, CancellationToken cancellationToken)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100; // Max 100 items per page

            // Flexible predicate structure for Admin
            var services = await _repository.FindAsync(s =>
                // 1. Filter by active status (if specified by Admin, ignore if null)
                (!request.IsActive.HasValue || s.IsActive == request.IsActive.Value) &&

                // 2. Filter by Keyword
                (string.IsNullOrEmpty(request.Keyword) ||
                 s.Name.ToLower().Contains(request.Keyword.ToLower()) ||
                 (s.Description != null && s.Description.ToLower().Contains(request.Keyword.ToLower()))) &&

                // 3. Filter by ServiceType
                (!request.ServiceType.HasValue || s.ServiceType == request.ServiceType.Value),

                // Include ServicePrices navigation property
                "ServicePrices"
            );

            // Calculate total count before pagination
            int totalCount = services.Count();

            // Apply pagination
            var paginatedServices = services
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTO
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
                Message = "Search services for admin successfully.",
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
