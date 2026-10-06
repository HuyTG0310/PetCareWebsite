using MediatR;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.DTOs.Pet;
using PetCareBooking.Domain.Enums;

namespace PetCareBooking.Application.Features.Pets.Queries.SearchPets
{
    public class SearchPetsQuery : IRequest<PagedResult<PetListResponseDTO>>
    {
        public string? Keyword { get; set; }
        public PetSpecies? Species { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsActive { get; set; }
    }
}
