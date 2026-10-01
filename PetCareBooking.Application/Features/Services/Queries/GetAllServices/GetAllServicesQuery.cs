using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs;

namespace PetCareBooking.Application.Features.Services.Queries.GetAllServices
{
    public class GetAllServicesQuery : IRequest<ApiResponse<IEnumerable<ServiceDTO>>>
    {
    }
}
