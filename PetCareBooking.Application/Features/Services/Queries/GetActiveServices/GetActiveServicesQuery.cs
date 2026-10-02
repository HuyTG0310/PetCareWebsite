using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;

namespace PetCareBooking.Application.Features.Services.Queries.GetActiveServices
{
    public class GetActiveServicesQuery : IRequest<ApiResponse<List<ServiceResponseDTO>>>
    {
    }
}
