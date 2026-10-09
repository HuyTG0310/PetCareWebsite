using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;

namespace PetCareBooking.Application.Features.Pets.Queries.GetPetsByCustomerId
{
    public class GetPetsByCustomerIdQuery : IRequest<ApiResponse<List<PetResponseDTO>>>
    {
        public Guid CustomerId { get; set; }
        public bool? IsActive { get; set; }
    }
}

