using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;

namespace PetCareBooking.Application.Features.Services.Queries.GetActiveServices
{
    public class GetActiveServicesQuery : IRequest<ApiResponse<PagedResult<ServiceListResponseDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
