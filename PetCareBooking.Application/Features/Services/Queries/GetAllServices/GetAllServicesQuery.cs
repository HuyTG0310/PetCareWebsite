using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;

namespace PetCareBooking.Application.Features.Services.Queries.GetAllServices
{
    public class GetAllServicesQuery : IRequest<ApiResponse<List<ServiceResponseDTO>>>
    {
    }
}
