using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Services.Queries.SearchServicesForAdmin
{
    public class SearchServicesForAdminQuery : IRequest<ApiResponse<PagedResult<ServiceListResponseDTO>>>
    {
        public string? Keyword { get; set; }
        public ServiceType? ServiceType { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
