using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;

namespace PetCareBooking.Application.Features.Pets.Queries.GetPetById
{
    public class GetPetByIdQuery : IRequest<ApiResponse<PetResponseDTO>>
    {
        public Guid Id { get; set; }

        public GetPetByIdQuery()
        {
        }

        public GetPetByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
