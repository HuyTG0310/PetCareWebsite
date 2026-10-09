using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Application.Interfaces;
using PetCareBooking.Domain.Entities;

namespace PetCareBooking.Application.Features.Services.Queries.GetActiveServices
{
    public class GetActiveServicesQueryHandler : IRequestHandler<GetActiveServicesQuery, ApiResponse<PagedResult<ServiceListResponseDTO>>>
    {
        private readonly IGenericRepository<Service> _repository;

        public GetActiveServicesQueryHandler(IGenericRepository<Service> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<ServiceListResponseDTO>>> Handle(GetActiveServicesQuery request, CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            var services = await _repository.FindAsync(x => x.IsActive == true, "RoomType");

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
                IsActive = s.IsActive,
                RoomTypeId = s.RoomTypeId,
                RoomTypeName = s.RoomType?.Name
            }).ToList();

            return new ApiResponse<PagedResult<ServiceListResponseDTO>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Get active services successfully.",
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
