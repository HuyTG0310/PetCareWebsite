using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Service;

namespace PetCareBooking.Application.Features.Services.Queries.GetServiceById
{
    public class GetServiceByIdQuery : IRequest<ApiResponse<ServiceResponseDTO>>
    {
        public Guid Id { get; set; }
    }
}
